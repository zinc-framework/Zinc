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
    public Rect Rect { get; private set; }
    public Rect SizeRect { get; private set; }
    public float Width => SizeRect.width;
    public float Height => SizeRect.height;
    public SpriteRenderer(Texture t, Rect r)
    {
        Texture = t;
        Rect = r;
        SizeRect = new Rect(0, 0, Rect.width, Rect.height);
    }

    public void UpdateRect(Rect r)
    {
        Rect = r;
        SizeRect = new Rect(0, 0, Rect.width, Rect.height);
    }
}

public record struct ShapeRenderer(Color Color, float Width, float Height, float PivotX, float PivotY) : IComponent;