namespace Zinc;

public class UseNestedComponentMemberNamesAttribute : Attribute {}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public class ComponentAttribute<T>(string name = "") : Attribute where T : IComponent {}