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
public record CollisionEvent(EntityReference e1, EntityReference e2);


public record struct Collider(float X, float Y, float Width, float Height, 
    bool Active = false,
    Action<EntityBase,EntityBase> OnStart = null, 
    Action<EntityBase,EntityBase> OnContinue = null, 
    Action<EntityBase,EntityBase> OnEnd = null,
    Action<EntityBase,List<Modifiers>> OnMouseUp = null,
    Action<EntityBase,List<Modifiers>> OnMousePressed = null,
    Action<EntityBase,List<Modifiers>> OnMouseDown = null,
    Action<EntityBase,List<Modifiers>,float,float> OnMouseScroll = null,
    Action<EntityBase,List<Modifiers>> OnMouseEnter = null,
    Action<EntityBase,List<Modifiers>> OnMouseExit = null,
    Action<EntityBase,List<Modifiers>> OnMouseOver = null
    ) : IComponent;