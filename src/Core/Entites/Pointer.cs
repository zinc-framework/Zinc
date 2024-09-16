using Arch.Core.Extensions;

namespace Zinc;

[Component<Position>]
[Component<Collider>("Collider")]
[Component<UpdateListener>]
public partial class Pointer : EntityBase
{
    private readonly Action<EntityBase, double>? _updateWrapper;
    public Pointer(bool startEnabled = true, Action<Pointer, double>? update = null) : base(startEnabled)
    {
        Collider_X = 0;
        Collider_Y = 0;
        Collider_Width = 1;
        Collider_Height = 1;
        Collider_Active = true;
        if (update != null)
        {
            _updateWrapper = (baseEntity, dt) => update((Pointer)baseEntity, dt);
            Update = _updateWrapper;
        }
    }
}