using Arch.Core;
using Arch.Core.Extensions;

namespace Zinc;

[Component<Collider>("Collider")]
[Component<RenderItem>]
[Component<SpriteRenderer>]
public partial class Sprite : Entity
{
    public SpriteData Data { get; init; }
    private readonly Action<EntityBase, double>? _updateWrapper;
    public Sprite(SpriteData spriteData, Scene? scene = null, bool startEnabled = true, Action<Sprite,double>? update = null)
        : base(startEnabled,scene)
    {
        Data = spriteData;
        RenderOrder = Scene.GetNextSceneRenderCounter();
        ECSEntity.Set(new SpriteRenderer(Data.Texture, Data.Rect));
        Collider_X = 0;
        Collider_Y = 0;
        Collider_Width = Data.Rect.width;
        Collider_Height = Data.Rect.height;
        Collider_Active = false;

        if (update != null)
        {
            _updateWrapper = (baseEntity, dt) => update((Sprite)baseEntity, dt);
            Update = _updateWrapper;
        }
    }
}