using System.Numerics;
using Arch.Core.Extensions;
using Zinc.Core;
using Zinc.Internal.Cute;
using Zinc.Internal.Sokol;
using Zinc.Internal.STB;
using Volatile;

namespace Zinc;

public class Entity
{
    public int ID;
    public string Name { get; set; } = "entity";
    public string DebugText = "";
    // public bool DestoryOnLoad = true;
    private float x = 0;
    public float X
    {
        get => x;
        set
        {
            ref var pos = ref ECSEntity.Get<Position>();
            pos.x = value;
            x = value;
        }
    }
    
    private bool active = true;
    public bool Active
    {
        get => active;
        set
        {
            ref var a = ref ECSEntity.Get<Active>();
            a.active = value;
            active = value;
        }
    }

    private float y = 0;
    public float Y
    {
        get => y;
        set
        {
            ref var pos = ref ECSEntity.Get<Position>();
            pos.y = value;
            y = value;
        }
    }
    
    //NOTE: maybe get rid of rotation/scale/pivot for an entity specifically?
    //they are more about component things instead of intrinsic entity things
    
    //also make physics and actual system to get rid of the jank setters
    private float scaleY = 1;
    public float ScaleY
    {
        get => scaleY;
        set
        {
            ref var pos = ref ECSEntity.Get<Position>();
            pos.scaleY = value;
            scaleY = value;
        }
    }
    
    private float scaleX = 1;
    public float ScaleX
    {
        get => scaleX;
        set
        {
            ref var pos = ref ECSEntity.Get<Position>();
            pos.scaleX = value;
            scaleX = value;
        }
    }
    
    private float rotation = 0;
    public float Rotation
    {
        get => rotation;
        set
        {
            ref var pos = ref ECSEntity.Get<Position>();
            pos.rotation = value;
            rotation = value;
        }
    }
    
    private float pivotX = 0;
    public float PivotX
    {
        get => pivotX;
        set
        {
            ref var pos = ref ECSEntity.Get<Position>();
            pos.pivotX = value;
            pivotX = value;
        }
    }
    
    private float pivotY = 0;
    public float PivotY
    {
        get => pivotY;
        set
        {
            ref var pos = ref ECSEntity.Get<Position>();
            pos.pivotY = value;
            pivotY = value;
        }
    }


    public void SetPositionRaw(float x, float y, float rotation, float scaleX, float scaleY, float pivotX, float pivotY)
    {
        this.x = x;
        this.y = y;
        this.scaleX = scaleX;
        this.scaleY = scaleY;
        this.rotation = rotation;
        this.pivotX = pivotX;
        this.pivotY = pivotY;
    }

    public void SetPosition(float x, float y, float rotation, float scaleX, float scaleY, float pivotX, float pivotY)
    {
        ref var pos = ref ECSEntity.Get<Position>();
        pos.x = x;
        pos.y = y;
        pos.scaleY = scaleY;
        pos.scaleX = scaleX;
        pos.rotation = rotation;
        pos.pivotX = pivotX;
        pos.pivotY = pivotY;
        SetPositionRaw(x,y,rotation,scaleX,scaleY,pivotX,pivotY);
    }
    
    public Arch.Core.EntityReference ECSEntityReference;
    public Arch.Core.Entity ECSEntity => ECSEntityReference.Entity;
    public Scene? Scene;
    public Entity(bool startEnabled, Scene scene, bool addToSceneHeirarchy = true, Action<Entity,double> update = null)
    {
        ID = Engine.GetNextEntityID();
        Scene = scene != null ? scene : Engine.TargetScene;
        Arch.Core.Entity e = addToSceneHeirarchy
            //in-scene entity
            ? Engine.ECSWorld.Create(
                new Active(startEnabled),
                new HasManagedOwner(this),
                new Position(X, Y),
                new UpdateListener(this, update),
                new SceneMember(Scene.ID)
            )
            //non-in-scene entity
            : Engine.ECSWorld.Create(
                new Active(startEnabled),
                new HasManagedOwner(this),
                new Position(X, Y),
                new UpdateListener(this, update));
        ECSEntityReference = Engine.ECSWorld.Reference(e);
        Engine.ECSWorld.Add<Entity>(ECSEntity);
        if (addToSceneHeirarchy)
        {
            Engine.SceneEntityMap[Scene].Add(this);
        }
    }
    
    public void Destroy()
    {
        if (!ECSEntity.Has<Destroy>())
        {
            ECSEntity.Add(new Destroy());
        }
    }

    public void DestroyImmediate()
    {
        Engine.SceneEntityMap[Scene].Remove(this);
        Engine.ECSWorld.Destroy(ECSEntity);
    }
}