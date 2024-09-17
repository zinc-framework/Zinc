namespace Zinc;

public readonly record struct Destroy();
public record struct ActiveState(bool Active = true) : IComponent;
// note ID here is the ID of the managed entity that "owns" the underlying ECS entity this component belongs to
// this also implicitly means that any entity that has EntityID is a managed entity
public record struct EntityID(int ID) : IComponent;
public record struct AdditionalEntityInfo(string Name = "Entity", string DebugText = "") : IComponent;