namespace Zinc;

public readonly record struct Destroy();
[Arch.AOT.SourceGenerator.Component]
public record struct ActiveState(bool Active = true) : IComponent;
// note ID here is the ID of the managed entity that "owns" the underlying ECS entity this component belongs to
// this also implicitly means that any entity that has EntityID is a managed entity
[Arch.AOT.SourceGenerator.Component]
public record struct EntityID(int ID) : IComponent;
[Arch.AOT.SourceGenerator.Component]
public record struct AdditionalEntityInfo(string Name = "Entity", string DebugText = "") : IComponent;