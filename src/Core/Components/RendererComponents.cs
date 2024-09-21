using System.Numerics;
using static Zinc.Resources;

namespace Zinc;

public record struct RenderItem(int RenderOrder) : IComponent;
public record struct SpriteRenderer : IComponent
{
    public Vector2 Pivot { get; set; }
    public float Rotation { get; set; }
    Texture texture;
    public Texture Texture => texture;
    private Rect rect;
    public Rect Rect 
    { 
        get => rect;
        set
        {
            rect = value;
            sizeRect = new Rect(rect.width, rect.height);
        }
    }
    Rect sizeRect = new Rect(0,0);
    public Rect SizeRect => sizeRect;
    public float Width => SizeRect.width;
    public float Height => SizeRect.height;
    public SpriteRenderer(Texture t, Rect r)
    {
        texture = t;
        Rect = r;
    }
}




public record struct ShapeRenderer(Color Color, float Width, float Height, float Rotation, Vector2 Pivot) : IComponent;
//TODO: make this actually work
public record struct TextRenderer(Color Color, string text,float Width, float Height) : IComponent;