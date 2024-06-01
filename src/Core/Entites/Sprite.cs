using Arch.Core;
using Arch.Core.Extensions;

namespace Zinc;

public class Sprite : Entity
{
    public SpriteData Data { get; init; }
    public Sprite(SpriteData spriteData, Scene? scene = null, bool startEnabled = true, 
        Action<Entity,double> update = null,
        Action<EntityReference,EntityReference> collisionStart = null, 
        Action<EntityReference,EntityReference> collisionStop = null, 
        Action<EntityReference,EntityReference> collisionContinue = null,
        Action<Arch.Core.Entity,List<Modifiers>> OnMouseUp = null,
        Action<Arch.Core.Entity,List<Modifiers>> OnMousePressed = null,
        Action<Arch.Core.Entity,List<Modifiers>> OnMouseDown = null,
        Action<Arch.Core.Entity,List<Modifiers>> OnMouseEnter = null,
        Action<Arch.Core.Entity,List<Modifiers>> OnMouseLeave = null,
        Action<Arch.Core.Entity,List<Modifiers>> OnMouseOver = null,
        Action<Arch.Core.Entity,List<Modifiers>,float,float> OnMouseScroll = null
        
        
    ) : base(startEnabled,scene,update:update)
    {
        Data = spriteData;
        sceneRenderOrder = Scene.GetNextSceneRenderCounter();
        var rend = new SpriteRenderer(Data.Texture, Data.Rect);
        ECSEntity.Add(
            rend,
            new RenderItem(sceneRenderOrder),
            new Collider(0,0,Data.Rect.width,Data.Rect.height,false,collisionStart,collisionContinue,collisionStop,OnMouseUp, OnMousePressed, OnMouseDown, OnMouseScroll,OnMouseEnter,OnMouseLeave,OnMouseOver));
    }
    
    private bool colliderActive;
    public bool ColliderActive
    {
        get => colliderActive;
        set
        {
            ref var c = ref ECSEntity.Get<Collider>();
            c.active = value;
            colliderActive = value;
        }
    }
    
    private int sceneRenderOrder;
    public int SceneRenderOrder
    {
        get => sceneRenderOrder;
        set
        {
            ref var r = ref ECSEntity.Get<RenderItem>();
            r.renderOrder = value;
            sceneRenderOrder = value;
        }
    }
}