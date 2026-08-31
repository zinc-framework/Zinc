namespace Zinc;

// The options Engine.Run takes. Split out of Core.cs alongside WindowOptions.
public static partial class Engine
{
    public record RunOptions(
        int width,
        int height,
        string appName,
        Action setup = null,
        Action update = null,
        bool imguiDockSpace = false,
        WindowOptions window = null,
        bool fullscreen = false);
}
