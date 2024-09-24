using Arch.Core;
using Arch.Core.Extensions;

namespace Zinc;

[Component<TemporaryComponent>]
[Component<RenderItem>]
[Component<ShapeRenderer>("Renderer")]
public partial class TemporaryShape : Anchor
{
    public TemporaryShape(float lifetime = 2, float width = 32, float height = 32, Color color = null, Scene? scene = null, bool startEnabled = true, Anchor? parent = null, List<Anchor>? children = null)
        : base(startEnabled,scene,parent:parent,children:children)
    {
        Lifetime = lifetime;
        Renderer_Pivot = new System.Numerics.Vector2(0.5f);
        Renderer_Color = color ?? Palettes.ENDESGA[9];
        Renderer_Width = width;
        Renderer_Height = height;
        RenderOrder = Scene.GetNextSceneRenderCounter();
    }
}

public record struct TemporaryComponent(float Lifetime = 2, double CurrentLife = 0) : IComponent;