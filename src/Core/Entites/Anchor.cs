using System.Numerics;
using Arch.Core.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Zinc.Core;
using Zinc.Internal.Cute;
using Zinc.Internal.Sokol;
using Zinc.Internal.STB;

namespace Zinc;

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

        PositionUpdated += (float dx, float dy) => {
            if(this.children.Count == 0)
            {
                return;
            }
            foreach (var child in this.children)
            {
                child.X += dx;
                child.Y += dy;
            }
        };
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
        // Don't allow parenting to null unless we're the scene root
        newParent = newParent ?? Engine.SceneLookup[SceneID];
        
        // Check for recursive parenting
        if (newParent != null && newParent.IsAncestor(this))
        {
            Console.WriteLine("WARNING!!: RECURSIVE PARENTING DETECTED------------------");
            Console.WriteLine($"Trying to assign parent for {Name} to: {newParent.Name}, but {Name} is already a child of {newParent.Name}");
            Console.WriteLine("------------------");
            return;
        }

        // Capture current world state
        var worldPos = GetWorldPosition();
        var pos = ECSEntity.Get<Position>();
        var currentRotation = pos.Rotation;  // Store current world rotation
        
        // Remove from old parent
        Parent?.children.Remove(this);
        
        // Add to new parent
        newParent.children.Add(this);
        Parent = newParent;
        
        // Calculate new local rotation
        if (Parent != null && !(Parent is Scene.SceneRootAnchor))
        {
            var parentRotation = Parent.ECSEntity.Get<Position>().Rotation;
            // Adjust local rotation to maintain world rotation
            pos.Rotation = currentRotation - parentRotation;
        }
        
        // Set position in new parent's space
        // This will handle any necessary rotation transformations
        SetWorldPosition(worldPos.X, worldPos.Y);
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

    public List<Anchor> AddChildren(List<Anchor> children)
    {
        foreach (var c in children)
        {
            AddChild(c);
        }
        return children;
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

    public Vector2 GetWorldPosition()
    {
        if (Parent == null || Parent is Scene.SceneRootAnchor)
            return new Vector2(X, Y);

        var parentWorld = Parent.GetWorldPosition();
        var localPos = new Vector2(X, Y);
        
        // Transform local position by parent's rotation if needed
        if (Parent.Rotation != 0)
        {
            var parentRotation = Matrix3x2.CreateRotation(Parent.Rotation);
            localPos = Vector2.Transform(localPos, parentRotation);
        }
        
        return parentWorld + localPos;
    }

    public void SetWorldPosition(float worldX, float worldY)
    {
        if (Parent == null || Parent is Scene.SceneRootAnchor)
        {
            X = worldX;
            Y = worldY;
            return;
        }

        var parentWorld = Parent.GetWorldPosition();
        var localOffset = new Vector2(worldX, worldY) - parentWorld;
        
        // Transform back to local space if parent is rotated
        if (Parent.Rotation != 0)
        {
            var inverseRotation = Matrix3x2.CreateRotation(-Parent.Rotation);
            localOffset = Vector2.Transform(localOffset, inverseRotation);
        }
        
        X = localOffset.X;
        Y = localOffset.Y;
    }

    public (Matrix3x2 transform, Vector2 scale) GetWorldTransform()
    {
        var pos = ECSEntity.Get<Position>();
        var (localRotation, _, localScale) = pos.GetTransform();
        
        // Start with local transform
        var localTransform = Matrix3x2.CreateRotation(pos.Rotation) * 
                            Matrix3x2.CreateTranslation(pos.X, pos.Y);

        if (Parent != null && !(Parent is Scene.SceneRootAnchor))
        {
            var (parentTransform, parentScale) = Parent.GetWorldTransform();
            
            // Combine with parent transform
            // Order is crucial: child transform * parent transform
            var worldTransform = localTransform * parentTransform;
            
            Vector2 worldScale = new Vector2(
                localScale.X * parentScale.X, 
                localScale.Y * parentScale.Y
            );

            return (worldTransform, worldScale);
        }

        return (localTransform, localScale);
    }
    
}