using Arch.Core;
using Arch.Core.Extensions;

namespace Zinc;

[Component<SpriteAnimator>]
public partial class AnimatedSprite : Sprite
{
    public new AnimatedSpriteData Data { get; init; }
    private readonly Action<Entity, double>? _updateWrapper;
    public AnimatedSprite(AnimatedSpriteData animatedSpriteData, Scene? scene = null, bool startEnabled = true, Action<AnimatedSprite,double>? update = null, Anchor? parent = null, List<Anchor>? children = null)
     : base(animatedSpriteData,scene,startEnabled,parent:parent,children:children)
    {
        Data = animatedSpriteData;
        RenderOrder = Scene.GetNextSceneRenderCounter();
        Animations = Data.Animations;
        var firstFrame = Animations.First().Frames[0];
        ECSEntity.Set(new SpriteRenderer(Data.Texture, firstFrame));
        Renderer_Pivot = new System.Numerics.Vector2(0.5f);
        Collider_Width = firstFrame.width;
        Collider_Height = firstFrame.height;
        Collider_Active = false;
        

        if (update != null && _updateWrapper == null)
        {
            _updateWrapper = (baseEntity, dt) => update((AnimatedSprite)baseEntity, dt);
            Update = _updateWrapper;
        }
    }
}