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

    public bool HasTag<T>() => Tags.Any(t => t is T);
    public bool GetTag<T>(out T tag)
    {
        foreach (var item in Tags)
        {
            if(item is T t)
            {
                tag = t;
                return true;
            }
        }
        tag = default!;
        return false;
    }
    public bool GetTags<T>(out List<T> tags)
    {
        tags = Tags.OfType<T>().ToList();
        return tags.Count > 0;
    }
    public bool Tagged(Tag tag) => Tags.Contains(tag);
    public bool Tagged(params Tag[] tags) => tags.All(Tags.Contains);
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

    /// <summary>
    /// Gets the children of this anchor. 
    /// </summary>
    /// <returns>A copy of the list of children</returns>
    public List<Anchor> GetChildren()
    {
        return children;
    }

    public void SetParent(Anchor newParent)
    {
        Parent.children.Remove(this);
        //TODO: figure out how to handle transfoms when changing parents
        newParent.children.Add(this);
        Parent = newParent;
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

    public Position LocalPosition => ECSEntity.Get<Position>();
    // original working with transforms not scaled
    // public (Matrix3x2 transform, Vector2 scale) GetWorldTransform()
    // {
    //     var (localTransform, localScale) = LocalPosition.GetLocalTransform();

    //     if (Parent != null)
    //     {
    //         var (parentTransform, parentScale) = Parent.GetWorldTransform();
            
    //         // Combine transforms and scales correctly
    //         return (localTransform * parentTransform, 
    //                 new Vector2(localScale.X * parentScale.X, localScale.Y * parentScale.Y));
    //     }

    //     return (localTransform, localScale);
    // }

    //working except for parent rotate due to scenerootachor
    public (Matrix3x2 transform, Vector2 scale) GetWorldTransform()
    {
        var (localRotation, localTranslation, localScale) = LocalPosition.GetLocalTransform();

        if (Parent != null && !(Parent is Scene.SceneRootAnchor))
        {
            var (parentTransform, parentScale) = Parent.GetWorldTransform();
            
            // Scale the local translation
            Vector2 scaledTranslation = new Vector2(
                localTranslation.X * parentScale.X,
                localTranslation.Y * parentScale.Y
            );

            // Create a matrix for the scaled translation
            Matrix3x2 scaledTranslationMatrix = Matrix3x2.CreateTranslation(scaledTranslation);

            // Combine transforms: (ScaledTranslation * LocalRotation) * ParentTransform
            Matrix3x2 worldTransform = (scaledTranslationMatrix * localRotation) * parentTransform;
            
            // Accumulate scale
            Vector2 worldScale = new Vector2(localScale.X * parentScale.X, localScale.Y * parentScale.Y);

            return (worldTransform, worldScale);
        }

        // If no parent, just combine local translation and rotation
        Matrix3x2 localTransform = localRotation * Matrix3x2.CreateTranslation(localTranslation);
        return (localTransform, localScale);
    }

    public Vector2 GetWorldPosition()
    {
        GetWorldTransform().transform.Decompose(out var translation, out var rotation, out var scale);
        // var (transform, scale) = GetWorldTransform().transform.Decompose();
        return translation;
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