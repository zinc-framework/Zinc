using Arch.Core;
using Arch.Core.Extensions;

namespace Zinc;

[Component<RenderItem>]
[Component<ShapeRenderer>]
[Component<Collider>("Collider")]
public partial class Shape : SceneEntity
{
    private readonly Action<Entity, double>? _updateWrapper;
    public Shape(Scene? scene = null, bool startEnabled = true, Action<Shape,double>? update = null, Anchor? parent = null, List<Anchor>? children = null)
        : base(startEnabled,scene,parent:parent,children:children)
    {
        Width = 32;
        Height = 32;
        RenderOrder = Scene.GetNextSceneRenderCounter();
        Collider_X = 0;
        Collider_Y = 0;
        Collider_Width = Width;
        Collider_Height = Height;
        Collider_Active = false;

        if (update != null)
        {
            _updateWrapper = (baseEntity, dt) => update((Shape)baseEntity, dt);
            Update = _updateWrapper;
        }
    }
}