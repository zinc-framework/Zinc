namespace Zinc;

// The options Engine.Run takes. Split out of Core.cs alongside WindowOptions.
public static partial class Engine
{
    /// <param name="highDpi">
    /// Render at the screen's own resolution: on a display scaled above 100% (Windows' display
    /// scale, a Retina Mac) <see cref="Width"/>/<see cref="Height"/> come out in physical pixels
    /// and <see cref="DPIScale"/> says by how much the OS is scaling everything else, so the app
    /// sizes its content by it. Off (the default), the app renders at the scaled-down size and
    /// the OS stretches the result over the window, which is softer but needs no handling.
    ///
    /// A <see cref="WindowOptions.Transparent"/> window on Windows wants this on: it is shown
    /// through DirectComposition, which doesn't stretch, so with it off the image only covers
    /// the window's top-left corner (2/3 of it each way at 150%) and the mouse, which is scaled,
    /// no longer lines up with what's drawn. Zinc warns at startup when that happens.
    ///
    /// Fixed when the window is created (it feeds sapp_desc.high_dpi).
    /// </param>
    public record RunOptions(
        int width,
        int height,
        string appName,
        Action setup = null,
        Action<double> update = null,
        Action shutdown = null,
        bool imguiDockSpace = false,
        WindowOptions window = null,
        bool fullscreen = false,
        bool highDpi = false);
}
