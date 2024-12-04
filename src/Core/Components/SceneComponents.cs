namespace Zinc;
[Arch.AOT.SourceGenerator.Component]
public record struct SceneMember(int SceneID) : IComponent;
[Arch.AOT.SourceGenerator.Component]
public readonly record struct SceneComponent() : IComponent;