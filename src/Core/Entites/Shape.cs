using Arch.Core;
using Arch.Core.Extensions;

namespace Zinc;

public class Shape : Entity
{
    private Color c;
    public Color Color
    {
        get => c;
        set
        {
            ref var sr = ref ECSEntity.Get<ShapeRenderer>();
            sr.Color = value;
            c = value;
        }
    }
    
    public Shape(Color color, int width = 32, int height = 32, Scene? scene = null, bool startEnabled = true, 
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
        c = color;
        var rend = new ShapeRenderer(color, width, height);
        sceneRenderOrder = Scene.GetNextSceneRenderCounter();
        ECSEntity.Add(
            rend,
            new RenderItem(sceneRenderOrder),
            new Collider(0,0,width,height,false,collisionStart,collisionContinue,collisionStop,OnMouseUp, OnMousePressed, OnMouseDown, OnMouseScroll,OnMouseEnter,OnMouseLeave,OnMouseOver));
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
    // [BindECSComponentValue<Collider>("active")]
    // private bool colliderActives;
}