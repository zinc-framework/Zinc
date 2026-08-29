namespace Zinc;

// Window shape settings. Split out of Core.cs so the knobs an app actually configures are
// easy to find; Engine is a static partial class, so these stay reachable as
// Engine.WindowOptions / Engine.Window exactly as before.
public static partial class Engine
{
    /// <summary>
    /// How the window is created and decorated. Pass one to RunOptions, and change it later
    /// with <see cref="ApplyWindowOptions"/>. The parameter defaults already describe an
    /// ordinary opaque window with a title bar, so <c>new WindowOptions()</c> is the plain
    /// case and there's no separate "default" instance to keep in sync with them.
    ///
    /// These are window *shape* settings. Transient behaviour like click-through is not here
    /// — see <see cref="Engine.ClickThrough"/>.
    /// </summary>
    /// <param name="Transparent">
    /// Composited, see-through background: the framebuffer is cleared to fully transparent
    /// each frame and whatever is behind the window shows through wherever nothing is drawn.
    /// Windows (D3D11, via DirectComposition) and macOS (Metal); elsewhere the window stays
    /// opaque and everything else still works.
    ///
    /// Unlike the other fields this is fixed when the window is created — it feeds
    /// sapp_desc.composite_mode, which sokol_app reads once — so ApplyWindowOptions can't
    /// change it afterwards.
    /// </param>
    /// <param name="Borderless">Drop the OS title bar and frame.</param>
    /// <param name="Topmost">Keep the window above normal windows.</param>
    /// <param name="ShowInTaskbar">Whether the window appears in the taskbar and Alt-Tab.</param>
    public record WindowOptions(
        bool Transparent = false,
        bool Borderless = false,
        bool Topmost = false,
        bool ShowInTaskbar = true)
    {
        /// <summary>
        /// The "desktop companion" shape: a see-through, frameless, always-on-top window that
        /// stays out of the taskbar, for something that lives on the desktop rather than in a
        /// window frame. Pair with <see cref="Engine.ClickThrough"/> for a purely decorative one.
        /// </summary>
        public static WindowOptions Companion { get; } =
            new(Transparent: true, Borderless: true, Topmost: true, ShowInTaskbar: false);
    }

    /// <summary>
    /// The options most recently passed to <see cref="ApplyWindowOptions"/>, or given to
    /// RunOptions at startup.
    ///
    /// This records what was *applied*; it is not a live query of the window. It stays
    /// accurate as long as the window's shape is only changed through ApplyWindowOptions.
    /// Reaching past it to <see cref="DesktopWindow"/> will leave it stale — that's the
    /// trade for DesktopWindow remaining available for one-off pokes.
    /// </summary>
    public static WindowOptions Window { get; private set; } = new();

    /// <summary>
    /// Apply a window shape and record it as <see cref="Window"/>. Usually used with a `with`
    /// expression to change one thing:
    ///
    /// <code>Engine.ApplyWindowOptions(Engine.Window with { Borderless = false });</code>
    ///
    /// Every field except <c>Transparent</c> can be changed at any time. Transparency is
    /// fixed at window creation, so a differing value is ignored with a warning and the real
    /// value is kept — leaving <see cref="Window"/> honest about the window that exists
    /// rather than the one that was asked for.
    ///
    /// Safe to call before the window exists: <see cref="DesktopWindow"/> reports failure
    /// instead of throwing, and Boot applies the options again once sokol_app has a window.
    /// </summary>
    public static void ApplyWindowOptions(WindowOptions options)
    {
        options ??= new WindowOptions();

        if (_windowCreated && options.Transparent != Window.Transparent)
        {
            Console.WriteLine(
                $"[Window] Transparent is fixed when the window is created (it is {Window.Transparent}); ignoring the change.");
            options = options with { Transparent = Window.Transparent };
        }

        DesktopWindow.Borderless = options.Borderless;
        DesktopWindow.Topmost = options.Topmost;
        DesktopWindow.ShowInTaskbar = options.ShowInTaskbar;

        Window = options;
    }

    /// <summary>
    /// False until sokol_app has created the window. Before that there's no native handle for
    /// the chrome calls to act on, and Transparent is still pending rather than settled.
    /// </summary>
    static bool _windowCreated;
}
