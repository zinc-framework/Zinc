namespace Zinc;

// Window creation and chrome settings. Split out of Core.cs so the knobs an app actually
// configures are easy to find; Engine is a static partial class, so these stay reachable
// as Engine.WindowOptions / Engine.Window exactly as before.
public static partial class Engine
{
    /// <summary>
    /// How the window itself is created and decorated. Pass one to RunOptions; the default
    /// is an ordinary opaque window with a title bar.
    ///
    /// These are window *creation and chrome* settings, fixed for the life of the window or
    /// changed deliberately through <see cref="DesktopWindow"/>. Transient behaviour like
    /// click-through is not here — see <see cref="Engine.ClickThrough"/>.
    /// </summary>
    /// <param name="Transparent">
    /// Composited, see-through background: the framebuffer is cleared to fully transparent
    /// each frame and whatever is behind the window shows through wherever nothing is drawn.
    /// Windows (D3D11, via DirectComposition) and macOS (Metal); elsewhere the window stays
    /// opaque and everything else still works. Must be decided before the window is created,
    /// which is why it lives here rather than on DesktopWindow.
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
        /// <summary>An ordinary opaque window with a title bar. Used when RunOptions doesn't name one.</summary>
        public static WindowOptions Default { get; } = new();
    }

    /// <summary>The WindowOptions this app was launched with. Never null once Boot has run.</summary>
    public static WindowOptions Window { get; private set; } = WindowOptions.Default;

    /// <summary>
    /// Shorthand for <c>Engine.Window.Transparent</c> — true when the framebuffer is cleared
    /// to transparent each frame rather than to the clear colour.
    /// </summary>
    public static bool TransparentWindow => Window.Transparent;
}
