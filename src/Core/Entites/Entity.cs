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
    public Anchor? Parent {get; private set; }= null;
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
            targetParent!.children.Add(this);
        }
        this.Parent = targetParent!;

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
        this.Parent.children.Remove(this);
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
        foreach (var c in children)
        {
            c.Destroy();
        }
        Engine.SceneEntityMap[SceneID].Remove(ID);
    }

    private Position LocalPosition => ECSEntity.Get<Position>();
    // private Matrix3x2 LocalTransform => LocalPosition; //implicit conversion to matrix
    private Matrix3x2 GetWorldTransform(Position? offset = null)
    {
        Matrix3x2 worldTransform = (Matrix3x2)LocalPosition;
        if(offset.HasValue)
        {
            worldTransform = worldTransform * (Matrix3x2)offset.Value; 
        }
        var currentAnchor = this;
        
        while (currentAnchor.Parent != null)
        {
            currentAnchor = currentAnchor.Parent;
            worldTransform = worldTransform * currentAnchor.LocalPosition;
        }

        return worldTransform;
    }

    public Position GetWorldPosition(Position? offset = null)
    {
        var worldTransform = GetWorldTransform(offset);
        // Extract position, scale, and rotation from the matrix
        System.Numerics.Vector2 position = worldTransform.Translation;

        // Method 1: Extract scale using vector length
        float scaleX = new System.Numerics.Vector2(worldTransform.M11, worldTransform.M21).Length();
        float scaleY = new System.Numerics.Vector2(worldTransform.M12, worldTransform.M22).Length();

        // Method 2: Extract scale using Matrix3x2.Multiply
        // var scaleMatrix = Matrix3x2.Multiply(Matrix3x2.CreateScale(((Matrix3x2)LocalPosition).GetDeterminant()), worldTransform);
        // float scaleX = scaleMatrix.M11;
        // float scaleY = scaleMatrix.M22;

        // Extract rotation correctly
        float rotation = (float)MathF.Atan2(worldTransform.M21, worldTransform.M11);

        // Normalize rotation to range [0, 2π)
        rotation = (rotation + 2 * MathF.PI) % (2 * MathF.PI);

        return new Position(position.X, position.Y, scaleX, scaleY, rotation);
    }
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