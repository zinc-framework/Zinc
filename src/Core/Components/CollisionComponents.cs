using System.Numerics;
using Arch.Core;

namespace Zinc;

public enum CollisionState
{
    Starting, //collision just started
    Continuing, //collision is still happening
    Ending, //collision just ended
    Invalid //collision no longer valid (one of the entities was destroyed as part of callbacks)
}

public record struct CollisionMeta(int hash, CollisionState state = CollisionState.Starting);
public record CollisionEvent(int entity1ManagedID, int entity2ManagedID);

[Arch.AOT.SourceGenerator.Component]
public record struct Collider(float Width, float Height, Vector2 Pivot,
    bool Active = false,
    Action<Entity, Entity> OnStart = null,
    Action<Entity, Entity> OnContinue = null,
    Action<Entity, Entity> OnEnd = null,
    Action<Entity, List<Modifiers>> OnMouseUp = null,
    Action<Entity, List<Modifiers>> OnMousePressed = null,
    Action<Entity, List<Modifiers>> OnMouseDown = null,
    Action<Entity, List<Modifiers>, float, float> OnMouseScroll = null,
    Action<Entity, List<Modifiers>> OnMouseEnter = null,
    Action<Entity, List<Modifiers>> OnMouseExit = null,
    Action<Entity, List<Modifiers>> OnMouseOver = null
    ) : IComponent
{
    // this collection is puppeted by the collision system
    public HashSet<Zinc.Entity> ActiveCollisions { get; internal set; } = new();
    public bool CollidingWith(Entity other) => ActiveCollisions.Contains(other);
    public bool CollidingWith<T>(out List<T> collisions) where T : Entity
    {
        collisions = new();
        foreach (var entity in ActiveCollisions)
        {
            if (entity is T)
            {
                collisions.Add((T)entity);
            }
        }
        return collisions.Count > 0;
    }
    public bool CollidingWithTagged(out List<Entity> collisions, params Tag[] tags)
    {
        collisions = new();
        foreach (var entity in ActiveCollisions)
        {
            if (entity.Tagged(tags))
            {
                collisions.Add(entity);
            }
        }
        return collisions.Count > 0;
    }
    public bool CollidingWithTagged<T>(out List<Entity> collisions) where T : Tag
    {
        collisions = new();
        foreach (var entity in ActiveCollisions)
        {
            if (entity.Tagged<T>())
            {
                collisions.Add(entity);
            }
        }
        return collisions.Count > 0;
    }
}
