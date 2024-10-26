using Zinc.Internal.Sokol;

namespace Zinc;

public class Color
{
    public ref float A => ref internal_color.a;
    public ref float R => ref internal_color.r;
    public ref float G => ref internal_color.g;
    public ref float B => ref internal_color.b;
    sg_color internal_color;
    public Color(uint hex)
    {
        internal_color = Internal.Sokol.Color.sg_make_color_1i(hex);
    }

    public Color(float r, float g, float b, float a)
    {
        internal_color.r = r;
        internal_color.g = g;
        internal_color.b = b;
        internal_color.a = a;
    }
    
    public static implicit operator Color(uint c)
    {
        return new Color(c);
    }
    
}