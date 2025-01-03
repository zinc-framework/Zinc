﻿using Arch.Core.Extensions;

namespace Zinc;

[Component<Position>]
[Component<Collider>("Collider")]
[Component<UpdateListener>]
public partial class Pointer : Entity
{
    private readonly Action<Entity, double>? _updateWrapper;
    public Pointer(bool startEnabled = true, Action<Pointer, double>? update = null) : base(startEnabled)
    {
        Collider_Pivot = new System.Numerics.Vector2(0.5f);
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