namespace Zinc;

public record struct SceneMember(int SceneID) : IComponent;
public readonly record struct SceneComponent() : IComponent;