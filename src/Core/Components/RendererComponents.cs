using Zinc.Core;
using System.Numerics;
using Arch.Core;
using Zinc.Internal.Sokol;
using static Zinc.Resources;

namespace Zinc;

public record struct RenderItem(int RenderOrder) : IComponent;
public record struct SpriteRenderer : IComponent
{
    public float PivotX;
    public float PivotY;
    public Texture Texture {get; private set;}
    private Rect rect;
    public Rect Rect 
    { 
        get => rect;
        set
        {
            Console.WriteLine($"Setting SizeRect to {rect.width}x{rect.height}");
            rect = value;
            SizeRect = new Rect(rect.width, rect.height);
        }
    }
    public Rect SizeRect { get; private set; } = new Rect(0,0);
    public float Width => SizeRect.width;
    public float Height => SizeRect.height;
    public SpriteRenderer(Texture t, Rect r)
    {
        Texture = t;
        Rect = r;
    }
}

public record struct ShapeRenderer(Color Color, float Width, float Height, float PivotX, float PivotY) : IComponent;