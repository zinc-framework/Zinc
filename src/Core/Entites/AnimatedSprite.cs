using Arch.Core;
using Arch.Core.Extensions;

namespace Zinc;

[Component<RenderItem>]
[Component<SpriteRenderer>]
[Component<Collider>("Collider")]
[Component<SpriteAnimator>]
public partial class AnimatedSprite : SceneEntity
{
    public AnimatedSpriteData Data { get; init; }
    private readonly Action<Entity, double>? _updateWrapper;
    public AnimatedSprite(AnimatedSpriteData animatedSpriteData, Scene? scene = null, bool startEnabled = true, Action<AnimatedSprite,double>? update = null, Anchor? parent = null, List<Anchor>? children = null)
     : base(startEnabled,scene,parent:parent,children:children)
    {
        Data = animatedSpriteData;
        RenderOrder = Scene.GetNextSceneRenderCounter();
        ECSEntity.Set(new SpriteRenderer(Data.Texture, Data.Animations.First().Frames[0]));
        Animations = Data.Animations;

        Collider_X = 0;
        Collider_Y = 0;
        Collider_Width = Data.Animations.First().Frames[0].width;
        Collider_Height = Data.Animations.First().Frames[0].height;
        Collider_Active = false;
        

        if (update != null)
        {
            _updateWrapper = (baseEntity, dt) => update((AnimatedSprite)baseEntity, dt);
            Update = _updateWrapper;
        }
    }
}