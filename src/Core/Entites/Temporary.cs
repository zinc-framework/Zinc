using Arch.Core;
using Arch.Core.Extensions;

namespace Zinc;

[Component<TemporaryComponent>]
public partial class TemporaryShape : Shape
{
    public TemporaryShape(float lifetime = 2, float width = 32, float height = 32, Color color = null, Scene? scene = null, bool startEnabled = true, Action<Shape,double> update = null, Anchor? parent = null, List<Anchor>? children = null)
        : base(width,height,color,scene,startEnabled,update,parent,children)
    {
        Lifetime = lifetime;
    }
}

public record struct TemporaryComponent(float Lifetime = 2, double CurrentLife = 0) : IComponent;