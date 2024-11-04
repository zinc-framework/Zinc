using System.Numerics;
using Arch.Core.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Zinc.Core;
using Zinc.Internal.Cute;
using Zinc.Internal.Sokol;
using Zinc.Internal.STB;

namespace Zinc;

public record Tag(string Value)
{
    public static implicit operator Tag(string value) => new(value);
}

[Component<EntityID>]
[Component<AdditionalEntityInfo>]
[Component<ActiveState>]
public partial class Entity
{
    public Arch.Core.EntityReference ECSEntityReference;
    public Arch.Core.Entity ECSEntity => ECSEntityReference.Entity;
    public HashSet<Tag> Tags = new();
    public Entity(bool startEnabled)
    {
        ECSEntityReference = Engine.ECSWorld.Reference(CreateECSEntity(Engine.ECSWorld));
        AssignDefaultValues();
        ID = Engine.GetNextEntityID();
        Engine.EntityLookup.Add(ID, this);
        Active = startEnabled;
    }

    public bool GetTags<T>(out List<T> tags)
    {
        tags = new List<T>();
        if(Tags.OfType<T>().Count() > 0)
        {
            tags = Tags.OfType<T>().ToList();
            return true;
        }
        return false;
    }
    public bool Tagged<T>() => Tags.Any(t => t is T);
    public bool Tagged(Tag tag) => Tags.Contains(tag);
    public bool Tagged(params Tag[] tags)
    {
        foreach (var tag in tags)
        {
            if(!Tags.Contains(tag))
            {
                return false;
            }
        }
        return true;
    }
    public bool NotTagged(params Tag[] tags)
    {
        foreach (var tag in tags)
        {
            if (Tags.Contains(tag))
            {
                return false;
            }
        }
        return true;
    }
    public void Tag(Tag tag) => Tags.Add(tag);
    public void Tag(params Tag[] tags) => Tags.UnionWith(tags);
    public void Untag(Tag tag) => Tags.Remove(tag);
    public void Untag(params Tag[] tags) => Tags.ExceptWith(tags);
    public bool StagedForDestruction => ECSEntity.Has<Destroy>();
    public void Destroy()
    {
        OnDestroy();
        ECSEntity.Add<Destroy>();
    }
    protected virtual void OnDestroy() {}
}

/// <summary>
/// A SceneObject is something that is associated with a scene
/// </summary>
[Component<SceneMember>]
public partial class SceneObject : Entity
{
    public Scene Scene => Engine.SceneLookup[SceneID];
    public SceneObject(bool startEnabled, Scene? scene = null) 
        : base(startEnabled)
    {
        SceneID = scene != null ? scene.ID : Engine.TargetScene.ID;
        Engine.SceneEntityMap[SceneID].Add(ID);
    }
    protected override void OnDestroy()
    {
        Engine.SceneEntityMap[SceneID].Remove(ID);
    }
}

/// <summary>
/// An anchor is a point in a scene
/// </summary>
[Component<Position>]
public partial class Anchor : SceneObject
{
    public Anchor? Parent {get; private set; } = null;
    private List<Anchor> children = new();

    // Local position properties calculated from world position
    public float LocalX
    {
        get => GetLocalPosition().X;
        set
        {
            SetLocalPosition(new Vector2(value, GetLocalPosition().Y));
        }
    }

    public float LocalY
    {
        get => GetLocalPosition().Y;
        set
        {
            SetLocalPosition(new Vector2(GetLocalPosition().X, value));
        }
    }

    public Anchor(bool startEnabled, Scene? scene = null, Anchor? parent = null, List<Anchor>? children = null) 
        : base(startEnabled,scene)
    {
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

private Vector2 GetLocalPosition()
{
    if (Parent == null || Parent is Scene.SceneRootAnchor)
        return new Vector2(X, Y);

    var worldPos = new Vector2(X, Y);
    var parentWorldPos = new Vector2(Parent.X, Parent.Y);
    
    // Get the difference between world positions
    Vector2 offset = worldPos - parentWorldPos;
    
    // If parent is rotated, we need to transform the offset back to parent's local space
    if (Parent.Rotation != 0)
    {
        var inverseRotation = Matrix3x2.CreateRotation(-Parent.Rotation);
        offset = Vector2.Transform(offset, inverseRotation);
    }
    
    return offset;
}

private void SetLocalPosition(Vector2 localPos)
{
    if (Parent == null || Parent is Scene.SceneRootAnchor)
    {
        X = localPos.X;
        Y = localPos.Y;
        return;
    }

    // Transform local position by parent's rotation
    Vector2 rotatedLocalPos = localPos;
    if (Parent.Rotation != 0)
    {
        var parentRotation = Matrix3x2.CreateRotation(Parent.Rotation);
        rotatedLocalPos = Vector2.Transform(localPos, parentRotation);
    }

    // Set world position as parent position plus transformed local offset
    var worldPos = new Vector2(Parent.X, Parent.Y) + rotatedLocalPos;
    X = worldPos.X;
    Y = worldPos.Y;
}

/// <summary>
    /// Gets the children of this anchor. 
    /// </summary>
    /// <returns>A copy of the list of children</returns>
    public List<Anchor> GetChildren(bool recursive = false)
    {
        var result = new List<Anchor>(children);
        if (recursive)
        {
            foreach (var child in children)
            {
                result.AddRange(child.GetChildren(true));
            }
        }
        return result;
    }

    public void SetParent(Anchor newParent)
    {
        //TODO: update position for new parent - this is a bit tricky


        // Don't allow parenting to null unless we're the scene root
        newParent = newParent ?? Engine.SceneLookup[SceneID];
        
        // Check for recursive parenting without using GetChildren
        if (newParent != null && newParent.IsAncestor(this))
        {
            Console.WriteLine("WARNING!!: RECURSIVE PARENTING DETECTED------------------");
            Console.WriteLine($"Trying to assign parent for {Name} to: {newParent.Name}, but {Name} is already a child of {newParent.Name}");
            Console.WriteLine("------------------");
            return; // Prevent the invalid parent assignment
        }

        // Remove from old parent
        Parent?.children.Remove(this);
        
        // Add to new parent
        newParent.children.Add(this);
        Parent = newParent;
    }

    private bool IsAncestor(Anchor potentialAncestor)
    {
        var current = this;
        while (current.Parent != null && current.Parent is not Scene.SceneRootAnchor)
        {
            if (current.Parent == potentialAncestor)
                return true;
            current = current.Parent;
        }
        return false;
    }

    public Anchor AddChild(Anchor child)
    {
        child.SetParent(this);
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
        base.OnDestroy();
    }

public (Matrix3x2 transform, Vector2 scale) GetWorldTransform()
{
    var pos = ECSEntity.Get<Position>();
    var (rotation, _, localScale) = pos.GetTransform();

    if (Parent != null && !(Parent is Scene.SceneRootAnchor))
    {
        var (parentTransform, parentScale) = Parent.GetWorldTransform();
        
        // Get current position relative to parent's position (not transform)
        var parentPos = new Vector2(Parent.X, Parent.Y);
        var thisPos = new Vector2(X, Y) - parentPos;
        
        // Build transform in parent space
        var translation = Matrix3x2.CreateTranslation(thisPos);
        var localTransform = rotation * translation;
        
        // Transform to world space by parent
        Matrix3x2 worldTransform = localTransform * parentTransform;
        
        Vector2 worldScale = new Vector2(
            localScale.X * parentScale.X, 
            localScale.Y * parentScale.Y
        );

        return (worldTransform, worldScale);
    }

    // If no parent, use world values directly
    var worldTranslation = Matrix3x2.CreateTranslation(pos.X, pos.Y);
    return (rotation * worldTranslation, localScale);
}
    
}


[Component<UpdateListener>]
public partial class SceneEntity : Anchor
{
    public SceneEntity(bool startEnabled, Scene? scene = null, Action<Entity, double>? update = null, Anchor? parent = null, List<Anchor>? children = null) 
        : base(startEnabled, scene, parent,children)
    {
        X = Engine.Width/2f;
        Y = Engine.Height/2f;
        Update = update;
    }
}