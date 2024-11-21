using Arch.Core;
using static Zinc.Internal.Sokol.Fontstash;

namespace Zinc.Core;

public static class Text
{
    // public enum FontState
    // {
    //     Unload,
    //     Loaded,
    //     Invalid
    // }
    // public record struct Font(int id, )
    public static Dictionary<string, int> fonts = new();
    public static int LoadFont(string path, string name)
    {
        if(fonts.ContainsKey(name))
        {
            return fonts[name];
        }
        unsafe
        {
            // byte[] fontNameBytes = System.Text.Encoding.UTF8.GetBytes("droid-regular");
            // byte[] fontDataBytes = File.ReadAllBytes($"{AppContext.BaseDirectory}/data/fonts/droidserif/DroidSerif-Regular.ttf");
            byte[] fontNameBytes = System.Text.Encoding.UTF8.GetBytes(name);
            byte[] fontDataBytes = File.ReadAllBytes(path);
            fixed (byte* n_p = fontNameBytes, fontBytes_p = fontDataBytes)
            {
                var loadedFont = fonsAddFontMem(Engine.font_state.FONSContext, (sbyte*)n_p, fontBytes_p, fontDataBytes.Length, 0);
                if(loadedFont == -1)
                {
                    Console.WriteLine("Failed to load font");
                    return -1;
                }
                else
                {
                    fonts.Add(name, loadedFont);
                    return loadedFont;
                }
            }
        }
    }
}