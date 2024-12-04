using System.Numerics;
using static Zinc.Resources;

namespace Zinc;
[Arch.AOT.SourceGenerator.Component]
public record struct RenderItem(int RenderOrder) : IComponent;
[Arch.AOT.SourceGenerator.Component]
public record struct SpriteRenderer : IComponent
{
    public Vector2 Pivot { get; set; }
    public Color Color {get; set;}
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
        Color = new Color(1.0f,1.0f,1.0f,1.0f);
    }
}



[Arch.AOT.SourceGenerator.Component]
public record struct ShapeRenderer(Color Color, float Width, float Height, Vector2 Pivot) : IComponent;
//TODO: make this actually work
[Arch.AOT.SourceGenerator.Component]
public record struct TextRenderer(Color Color, string fontPath, string text, float size, float spacing, float blur, Vector2 Pivot) : IComponent;