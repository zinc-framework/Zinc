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
    /// <summary>How this sprite blends with what's behind it; sprites default to alpha Blend.</summary>
    public BlendMode BlendMode { get; set; }

    /// <summary>
    /// Mirror the sprite left-to-right / top-to-bottom as it is drawn.
    ///
    /// This is a texture-space flip, not a transform: the source region is read backwards while
    /// the quad, the pivot and the scale are left exactly as they are. So it is independent of
    /// all three - a flipped sprite occupies the same pixels as an unflipped one whatever the
    /// pivot happens to be, which is not true of the negative-scale trick (that mirrors about
    /// the pivot, so it only stays in place while the pivot is centred).
    ///
    /// Flipping is applied before rotation, the same ordering every 2D engine's sprite-flip uses:
    /// the artwork mirrors within the quad, then the quad rotates.
    /// </summary>
    public bool FlipX { get; set; }
    /// <inheritdoc cref="FlipX"/>
    public bool FlipY { get; set; }
    public SpriteRenderer(Texture t, Rect r)
    {
        texture = t;
        Rect = r;
        Color = new Color(1.0f,1.0f,1.0f,1.0f);
        BlendMode = BlendMode.Blend;
    }
}



[Arch.AOT.SourceGenerator.Component]
public record struct ShapeRenderer(Color Color, float Width, float Height, Vector2 Pivot) : IComponent
{
    /// <summary>How this shape blends with what's behind it; shapes default to None (opaque).</summary>
    public BlendMode BlendMode { get; set; }
}
//TODO: make this actually work
[Arch.AOT.SourceGenerator.Component]
public record struct TextRenderer(Color Color, string fontPath, string text, float size, float spacing, float blur, Vector2 Pivot) : IComponent;