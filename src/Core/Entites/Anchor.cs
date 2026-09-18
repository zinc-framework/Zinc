using System.Numerics;
using Arch.Core.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Zinc.Core;
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
                // Don't preserve world position - children passed to constructor
                // should keep their local positions relative to this new parent
                AddChild(c, preserveWorldPosition: false);
            }
        }
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

    public void SetParent(Anchor newParent, bool preserveWorldPosition = true)
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

        // Capture current world state (only needed if preserving)
        Vector2 worldPos = default;
        float currentRotation = 0;
        if (preserveWorldPosition)
        {
            worldPos = GetWorldPosition();
            var pos = ECSEntity.Get<Position>();
            currentRotation = pos.Rotation;
        }

        // Remove from old parent
        Parent?.children.Remove(this);

        // Add to new parent
        newParent.children.Add(this);
        Parent = newParent;

        if (preserveWorldPosition)
        {
            // Calculate new local rotation
            if (Parent != null && !(Parent is Scene.SceneRootAnchor))
            {
                var parentRotation = Parent.ECSEntity.Get<Position>().Rotation;
                ref var pos = ref ECSEntity.Get<Position>();
                // Adjust local rotation to maintain world rotation
                pos.Rotation = currentRotation - parentRotation;
            }

            // Set position in new parent's space
            // This will handle any necessary rotation transformations
            SetWorldPosition(worldPos.X, worldPos.Y);
        }
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

    public Anchor AddChild(Anchor child, bool preserveWorldPosition = true)
    {
        child.SetParent(this, preserveWorldPosition);
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

    // Same answer as GetWorldTransform, which is the one place the hierarchy is walked: what gets
    // drawn, what collides and what this reports can't drift apart.
    public Vector2 GetWorldPosition() => GetWorldTransform().transform.Translation;

    public void SetWorldPosition(float worldX, float worldY)
    {
        if (Parent == null || Parent is Scene.SceneRootAnchor)
        {
            X = worldX;
            Y = worldY;
            return;
        }

        // Undo GetWorldTransform: out of the parent's rotation + translation, then out of its scale.
        // The parent matrix never carries scale, so it always inverts.
        var (parentTransform, parentScale) = Parent.GetWorldTransform();
        Matrix3x2.Invert(parentTransform, out var toParentSpace);
        var localOffset = Vector2.Transform(new Vector2(worldX, worldY), toParentSpace);

        // a parent squashed to zero on an axis puts every offset on that axis at the same spot
        X = parentScale.X != 0 ? localOffset.X / parentScale.X : 0;
        Y = parentScale.Y != 0 ? localOffset.Y / parentScale.Y : 0;
    }

    public (Matrix3x2 transform, Vector2 scale) GetWorldTransform()
    {
        var pos = ECSEntity.Get<Position>();
        var (localRotation, _, localScale) = pos.GetTransform();

        if (Parent != null && !(Parent is Scene.SceneRootAnchor))
        {
            var (parentTransform, parentScale) = Parent.GetWorldTransform();

            // A child's offset is measured in its parent's units, so it takes on the parent's
            // world scale just like the child's size does: scale the parent and the whole group
            // scales. Scale stays out of the matrix itself (carried alongside instead) so a
            // rotated child under a non-uniform parent stays a rectangle rather than shearing.
            var localTransform = Matrix3x2.CreateRotation(pos.Rotation) *
                                Matrix3x2.CreateTranslation(pos.X * parentScale.X, pos.Y * parentScale.Y);

            // Combine with parent transform
            // Order is crucial: child transform * parent transform
            var worldTransform = localTransform * parentTransform;

            Vector2 worldScale = new Vector2(
                localScale.X * parentScale.X, 
                localScale.Y * parentScale.Y
            );

            return (worldTransform, worldScale);
        }

        return (Matrix3x2.CreateRotation(pos.Rotation) * Matrix3x2.CreateTranslation(pos.X, pos.Y), localScale);
    }
    
}