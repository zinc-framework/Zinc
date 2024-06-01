using Zinc.Internal.Sokol;

namespace Zinc;

public class Color
{
    public float A
    {
        get => internal_color.a;
        set
        {
            var internalColor = internal_color;
            internalColor.a = value;
            internal_color = internalColor;
        }
    }
    
    public float R
    {
        get => internal_color.r;
        set
        {
            var internalColor = internal_color;
            internalColor.r = value;
            internal_color = internalColor;
        }
    }
    
    public float G
    {
        get => internal_color.g;
        set
        {
            var internalColor = internal_color;
            internalColor.g = value;
            internal_color = internalColor;
        }
    }
    
    public float B
    {
        get => internal_color.b;
        set
        {
            var internalColor = internal_color;
            internalColor.b = value;
            internal_color = internalColor;
        }
    }

    public sg_color internal_color {get; private set;}
    public Color(uint hex)
    {
        internal_color = Internal.Sokol.Color.sg_make_color_1i(hex);
    }

    public Color(float a, float r, float g, float b)
    {
        internal_color = new sg_color()
        {
            a = a,
            r = r,
            g = g,
            b = b
        };
    }
    
    public static implicit operator Color(uint c)
    {
        return new Color(c);
    }
    
}