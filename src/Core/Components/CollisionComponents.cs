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
    Action<EntityReference,EntityReference> OnStart = null, 
    Action<EntityReference,EntityReference> OnContinue = null, 
    Action<EntityReference,EntityReference> OnEnd = null,
    Action<Arch.Core.Entity,List<Modifiers>> OnMouseUp = null,
    Action<Arch.Core.Entity,List<Modifiers>> OnMousePressed = null,
    Action<Arch.Core.Entity,List<Modifiers>> OnMouseDown = null,
    Action<Arch.Core.Entity,List<Modifiers>,float,float> OnMouseScroll = null,
    Action<Arch.Core.Entity,List<Modifiers>> OnMouseEnter = null,
    Action<Arch.Core.Entity,List<Modifiers>> OnMouseExit = null,
    Action<Arch.Core.Entity,List<Modifiers>> OnMouseOver = null
    ) : IComponent;