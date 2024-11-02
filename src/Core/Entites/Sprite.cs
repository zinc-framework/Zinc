using Arch.Core;
using Arch.Core.Extensions;

namespace Zinc;

[Component<Collider>("Collider")]
[Component<RenderItem>]
[Component<SpriteRenderer>("Renderer")]
public partial class Sprite : SceneEntity
{
    public SpriteData Data { get; init; }
    private readonly Action<Entity, double>? _updateWrapper;
    public Sprite(SpriteData spriteData, Scene? scene = null, bool startEnabled = true, Action<Sprite,double>? update = null, Anchor? parent = null, List<Anchor>? children = null)
        : base(startEnabled,scene,parent:parent,children:children)
    {
        Data = spriteData; 
        Renderer_Color = new Color(1.0f,1.0f,1.0f,1.0f);
        RenderOrder = Scene.GetNextSceneRenderCounter();
        ECSEntity.Set(new SpriteRenderer(Data.Texture, Data.Rect));
        Renderer_Pivot = new System.Numerics.Vector2(0.5f);
        Collider_Pivot = new System.Numerics.Vector2(0.5f);
        Collider_Width = Data.Rect.width;
        Collider_Height = Data.Rect.height;
        Collider_Active = false;

        if (update != null  && _updateWrapper == null)
        {
            _updateWrapper = (baseEntity, dt) => update((Sprite)baseEntity, dt);
            Update = _updateWrapper;
        }
    }
}