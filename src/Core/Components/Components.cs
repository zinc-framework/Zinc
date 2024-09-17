using Zinc.Core;
using System.Numerics;
using Arch.Core;
using Zinc.Internal.Sokol;
using static Zinc.Resources;

namespace Zinc;

public class UseNestedComponentMemberNamesAttribute : System.Attribute {}
public interface IComponent {}
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public class ComponentAttribute<T>(string name = "") : System.Attribute where T : IComponent {}


public record struct Position(float X = 0, float Y = 0, float ScaleX = 1, float ScaleY = 1, float Rotation = 0f) : IComponent;
public readonly record struct Destroy();
public record struct ActiveState(bool Active = true) : IComponent;
// note ID here is the ID of the managed entity that "owns" the underlying ECS entity this component belongs to
// this also implicitly means that any entity that has EntityID is a managed entity
public readonly record struct EntityID(int ID) : IComponent;
public record struct AdditionalEntityInfo(string Name = "Entity", string DebugText = "") : IComponent;
public record struct SceneMember(int SceneID) : IComponent;
public readonly record struct SceneComponent(int SceneID) : IComponent;

//note that update needs to be tied to a managed entity
//its assumed that if you are making raw ecs entities you are working with systems
public record struct UpdateListener(Action<Zinc.EntityBase,double> Update) : IComponent;