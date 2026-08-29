using System.Runtime.InteropServices;
using Zinc.Internal.Sokol;

namespace Zinc;

/// <summary>
/// Window-chrome controls for "desktop companion" apps: a frameless, always-on-top,
/// optionally see-through window that lives on the desktop rather than in a normal
/// window frame.
///
/// sokol_app has no concept of an undecorated window — it always creates a normal titled
/// frame — but it does hand out the native window handle, so the restyling happens in the
/// zinc_platform native lib instead of in our sokol fork. See
/// Zinc.Bootstrapper/libs/zinc_platform/build/zinc_window.{c,m}.
///
/// actions and queries that carry no state: dragging, positioning, work-area lookup.
///
/// See <see cref="Engine.WindowOptions"/> for the shape settings themselves.
///
/// Every call is safe on any platform and simply reports false where the platform doesn't
/// implement it. Currently: Windows implemented, macOS written but not yet verified on
/// hardware, Linux stubbed.
///
/// The window-shape calls are internal on purpose: they're stateless pokes at the native
/// window, and Engine.ApplyWindowOptions is the only thing that should drive them, so that
/// Engine.Window can't disagree with the real window. What stays public here are the
/// </summary>
public static class DesktopWindow
{
    const string LIB = "zinc_platform";

    [DllImport(LIB, EntryPoint = "zinc_window_set_borderless", CallingConvention = CallingConvention.Cdecl)]
    static extern unsafe int zinc_window_set_borderless(void* handle, int borderless);

    [DllImport(LIB, EntryPoint = "zinc_window_set_topmost", CallingConvention = CallingConvention.Cdecl)]
    static extern unsafe int zinc_window_set_topmost(void* handle, int topmost);

    [DllImport(LIB, EntryPoint = "zinc_window_set_taskbar_visible", CallingConvention = CallingConvention.Cdecl)]
    static extern unsafe int zinc_window_set_taskbar_visible(void* handle, int visible);

    [DllImport(LIB, EntryPoint = "zinc_window_begin_drag", CallingConvention = CallingConvention.Cdecl)]
    static extern unsafe int zinc_window_begin_drag(void* handle);

    [DllImport(LIB, EntryPoint = "zinc_window_set_click_through", CallingConvention = CallingConvention.Cdecl)]
    static extern unsafe int zinc_window_set_click_through(void* handle, int enable);

    [DllImport(LIB, EntryPoint = "zinc_window_restore_wndproc", CallingConvention = CallingConvention.Cdecl)]
    static extern unsafe int zinc_window_restore_wndproc(void* handle);

    [DllImport(LIB, EntryPoint = "zinc_window_set_position", CallingConvention = CallingConvention.Cdecl)]
    static extern unsafe int zinc_window_set_position(void* handle, int x, int y);

    [DllImport(LIB, EntryPoint = "zinc_window_get_work_area", CallingConvention = CallingConvention.Cdecl)]
    static extern unsafe int zinc_window_get_work_area(void* handle, int* x, int* y, int* w, int* h);

    /// <summary>
    /// The native window handle sokol created: an HWND on Windows, an NSWindow* on macOS.
    /// Null before the window exists (i.e. before Engine.Boot reaches App.run).
    /// </summary>
    static unsafe void* NativeHandle
    {
        get
        {
            if (OperatingSystem.IsWindows()) return App.win32_get_hwnd();
            if (OperatingSystem.IsMacOS()) return App.macos_get_window();
            return null;
        }
    }

    static unsafe bool Call(Func<IntPtr, int> f)
    {
        void* h = NativeHandle;
        if (h == null) return false;
        try { return f((IntPtr)h) != 0; }
        catch (DllNotFoundException) { return false; }
        catch (EntryPointNotFoundException) { return false; }
    }


    /// <summary>
    /// Remove the OS title bar and frame, or put them back. The *client* area keeps its size
    /// across the change, so the rendered surface doesn't jump.
    ///
    /// NOTE on Windows: sokol's fullscreen path rewrites the window style wholesale, so if
    /// you toggle fullscreen after going borderless, apply the options again afterwards.
    /// </summary>
    internal static bool SetBorderless(bool borderless) =>
        Call(h => zinc_window_set_borderless((void*)h, borderless ? 1 : 0));


    /// <summary>Keep the window above normal windows, or stop doing so.</summary>
    internal static bool SetTopmost(bool topmost) =>
        Call(h => zinc_window_set_topmost((void*)h, topmost ? 1 : 0));


    /// <summary>
    /// Whether the window appears in the taskbar / Alt-Tab. Windows only — on macOS, Dock
    /// presence is an application-wide setting rather than a per-window one, so this reports
    /// false there and changes nothing.
    /// </summary>
    internal static bool SetTaskbarVisible(bool visible) =>
        Call(h => zinc_window_set_taskbar_visible((void*)h, visible ? 1 : 0));


    /// <summary>
    /// Let mouse input pass through to whatever is behind the window, for a purely
    /// decorative companion. Works in both opaque and transparent modes.
    ///
    /// While this is on the window receives NO mouse input at all — its own UI included,
    /// which is inherent to click-through rather than a limitation. Turn it off to interact
    /// with the window again, so leave yourself a keyboard route back.
    ///
    /// Implemented on Windows with WS_EX_LAYERED | WS_EX_TRANSPARENT. Both are required:
    /// WS_EX_TRANSPARENT alone does nothing for input, and WM_NCHITTEST/HTTRANSPARENT only
    /// forwards hit-tests to windows in the same thread, so it can't reach another
    /// application at all. Verified working in both opaque and transparent modes.
    /// </summary>
    internal static bool SetClickThrough(bool enable) =>
        Call(h => zinc_window_set_click_through((void*)h, enable ? 1 : 0));

    static bool _dragRequested;

    /// <summary>
    /// Hand the window to the OS's own move loop, so the user can drag the window by its
    /// content. A frameless window has no title bar to grab, so call this when you decide a
    /// drag has started (typically on mouse-down somewhere that isn't a UI widget).
    ///
    /// Letting the window manager run the drag — rather than repositioning per mouse-move —
    /// is what makes it feel native: it snaps, survives DPI changes, and doesn't lag.
    ///
    /// The drag is DEFERRED to the end of the current frame rather than started immediately.
    /// The OS move loop is a nested, blocking message loop: sokol_app keeps pumping frames
    /// from inside it, so starting one midway through a frame re-enters ImGui::NewFrame()
    /// before the outer frame has ended and trips
    ///   "Forgot to call Render() or EndFrame() at the end of the previous frame?".
    /// Deferring means the loop always begins on a frame boundary. This is why it is safe to
    /// call from UI code and input callbacks, which all run mid-frame.
    /// </summary>
    /// <returns>false if there's no native window yet; true if the drag was queued.</returns>
    public static unsafe bool BeginDrag()
    {
        if (NativeHandle == null) return false;
        _dragRequested = true;
        return true;
    }

    /// <summary>
    /// Runs a drag queued by <see cref="BeginDrag"/>. Called by Engine.Frame() once the
    /// frame is fully rendered and committed, so the OS move loop starts cleanly.
    /// </summary>
    internal static void PumpDeferredDrag()
    {
        if (!_dragRequested) return;
        _dragRequested = false;
        Call(h => zinc_window_begin_drag((void*)h));
    }

    /// <summary>
    /// Undo the window subclass installed by <see cref="ClickThrough"/>. Only needed if the
    /// native lib could be unloaded while the window lives on; normal shutdown doesn't
    /// require it. No-op on macOS, which needs no subclass.
    /// </summary>
    public static bool RestoreWindowProc() => Call(h => zinc_window_restore_wndproc((void*)h));

    /// <summary>Move the window's top-left corner to a screen position, in physical pixels.</summary>
    public static bool SetPosition(int x, int y) => Call(h => zinc_window_set_position((void*)h, x, y));

    /// <summary>
    /// Usable desktop area of the monitor the window is on, excluding the taskbar/Dock and
    /// menu bar, in physical pixels with a top-left origin. Returns false (and zeroes) if
    /// the platform can't report it.
    /// </summary>
    public static unsafe bool GetWorkArea(out int x, out int y, out int width, out int height)
    {
        x = y = width = height = 0;
        void* h = NativeHandle;
        if (h == null) return false;
        int lx, ly, lw, lh;
        int ok;
        try { ok = zinc_window_get_work_area(h, &lx, &ly, &lw, &lh); }
        catch (DllNotFoundException) { return false; }
        catch (EntryPointNotFoundException) { return false; }
        if (ok == 0) return false;
        x = lx; y = ly; width = lw; height = lh;
        return true;
    }

}
