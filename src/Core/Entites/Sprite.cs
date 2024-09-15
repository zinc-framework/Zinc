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
        Texture = Data.Texture;
        Rect = Data.Rect;
        //TODO: how do we call functions on the component from the entity?
        // var rend = new SpriteRenderer(Data.Texture, Data.Rect);
        // the above calls the ctor with a chained update call, but we have to call it here explicitly for setup
        ECSEntity.Get<SpriteRenderer>().UpdateRect(Data.Rect);
        Collider_X = 0;
        Collider_Y = 0;
        Collider_Width = Data.Rect.width;
        Collider_Height = Data.Rect.height;
        Collider_Active = false;

        if (update != null)
        {
            _updateWrapper = (baseEntity, dt) =>
            {
                update((Sprite)baseEntity, dt);
            };
            Update = _updateWrapper;
        }
    }
}