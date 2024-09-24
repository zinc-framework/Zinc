using Arch.Core;
using Arch.Core.Extensions;

namespace Zinc;

[Component<RenderItem>]
[Component<ShapeRenderer>("Renderer")]
[Component<Collider>("Collider")]
public partial class Shape : SceneEntity
{
    private readonly Action<Entity, double>? _updateWrapper;
    public Shape(float width = 32, float height = 32, Color color = null, Scene? scene = null, bool startEnabled = true, Action<Shape,double>? update = null, Anchor? parent = null, List<Anchor>? children = null)
        : base(startEnabled,scene,parent:parent,children:children)
    {
        Renderer_Pivot = new System.Numerics.Vector2(0.5f);
        Collider_Pivot = new System.Numerics.Vector2(0.5f);
        Renderer_Color = color ?? Palettes.ENDESGA[9];
        Renderer_Width = width;
        Renderer_Height = height;
        Collider_Width = width;
        Collider_Height = height;
        RenderOrder = Scene.GetNextSceneRenderCounter();
        Collider_Active = false;

        if (update != null && _updateWrapper == null)
        {
            _updateWrapper = (baseEntity, dt) => update((Shape)baseEntity, dt);
            Update = _updateWrapper;
        }
    }
}