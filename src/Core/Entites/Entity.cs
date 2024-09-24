using System.Numerics;
using Arch.Core.Extensions;
using Zinc.Core;
using Zinc.Internal.Cute;
using Zinc.Internal.Sokol;
using Zinc.Internal.STB;
using Volatile;

namespace Zinc;

[Component<EntityID>]
[Component<AdditionalEntityInfo>]
[Component<ActiveState>]
public partial class Entity
{
    public Arch.Core.EntityReference ECSEntityReference;
    public Arch.Core.Entity ECSEntity => ECSEntityReference.Entity;
    public Entity(bool startEnabled)
    {
        ECSEntityReference = Engine.ECSWorld.Reference(CreateECSEntity(Engine.ECSWorld));
        AssignDefaultValues();
        ID = Engine.GetNextEntityID();
        Engine.EntityLookup.Add(ID, this);
        Active = startEnabled;
    }
    public bool StagedForDestruction => ECSEntity.Has<Destroy>();
    public void Destroy()
    {
        OnDestroy();
        ECSEntity.Add<Destroy>();
    }
    protected virtual void OnDestroy() {}
}

/// <summary>
/// An anchor is a point in a scene
/// </summary>
[Component<Position>]
[Component<SceneMember>]
public partial class Anchor : Entity
{
    public Scene Scene => Engine.SceneLookup[SceneID];
    public Anchor? Parent {get; private set; } = null;
    private List<Anchor> children = new();
    public Anchor(bool startEnabled, Scene? scene = null, Anchor? parent = null, List<Anchor>? children = null) 
        : base(startEnabled)
    {
        SceneID = scene != null ? scene.ID : Engine.TargetScene.ID;
        Engine.SceneEntityMap[SceneID].Add(ID);
        Anchor? targetParent = null;
        if(!(this is Scene.SceneRootAnchor))
        {
            //we add ourselves to either the passed in parent or the root of the scene we are in
            //only scene root anchors get a null parent
            targetParent = parent != null ? parent : Engine.SceneLookup[SceneID];
            targetParent.children.Add(this);
        }
        this.Parent = targetParent;

        if(children != null)
        {
            foreach (var c in children)
            {
                AddChild(c);
            }
        }
    }

    public void SetParent(Anchor parent)
    {
        Parent.children.Remove(this);
        parent.children.Add(this);
    }

    public Anchor AddChild(Anchor child)
    {
        child.SetParent(this);
        children.Add(child);
        return child;
    }

    protected override void OnDestroy()
    {
        var currentChildren = new List<Anchor>(children);
        foreach (var c in currentChildren)
        {
            c.Destroy();
        }
        if(Parent != null)
        {
            Parent.children.Remove(this);
        }
        Engine.SceneEntityMap[SceneID].Remove(ID);
    }

    public Position LocalPosition => ECSEntity.Get<Position>();
    // private Matrix3x2 LocalTransform => LocalPosition; //implicit conversion to matrix
    public Matrix4x4 GetWorldTransform()
    {
        if (Parent != null)
        {
            return LocalPosition * Parent.GetWorldTransform();
        }

        return LocalPosition;
    }

    public Position GetWorldPosition(Position? offset = null) => GetWorldTransform().ToWorldPosition();
}


[Component<UpdateListener>]
public partial class SceneEntity : Anchor
{
    public SceneEntity(bool startEnabled, Scene? scene = null, Action<Entity, double>? update = null, Anchor? parent = null, List<Anchor>? children = null) 
        : base(startEnabled, scene, parent,children)
    {
        Update = update;
    }
}