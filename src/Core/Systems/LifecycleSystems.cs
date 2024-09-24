using System.Collections;
using System.Diagnostics;
using System.Numerics;
using Arch.CommandBuffer;
using Arch.Core;
using Arch.Core.Extensions;
using Zinc.Core;
using Zinc.Core.ImGUI;
using Zinc.Internal.Sokol;

namespace Zinc;

public class DestructionSystem : DSystem
{
    QueryDescription query = new QueryDescription().WithAll<Destroy>();
    QueryDescription collisionEvents = new QueryDescription().WithAll<CollisionEvent,CollisionMeta>();
    QueryDescription managedCleanupQuery = new QueryDescription().WithAll<Destroy,EntityID>();
    public void DestroyObjects()
    {
        Engine.ECSWorld.Query(in collisionEvents, (Arch.Core.Entity e, ref CollisionEvent ce, ref CollisionMeta cm) =>
        {
            if((    Engine.TryGetEntity(ce.entity1ManagedID, out var e1) && e1.StagedForDestruction) 
                ||  Engine.TryGetEntity(ce.entity2ManagedID, out var e2) && e2.StagedForDestruction)
            {
                e.Add(new Destroy());
            }
        });

        Engine.ECSWorld.Query(in managedCleanupQuery, (Arch.Core.Entity e, ref EntityID owner) =>
        {
            Engine.EntityLookup.Remove(owner.ID);
        });

        Engine.ECSWorld.Destroy(in query);
    }
}

public class EventCleaningSystem : DSystem, ICleanupSystem
{
    private QueryDescription events = new QueryDescription().WithAll<EventMeta>();
    public void Cleanup()
    {
        Engine.ECSWorld.Query(in events,
            (Arch.Core.Entity e, ref EventMeta m) =>
            {
                if (m.dirty)
                {
                    e.Add(new Destroy());
                }
                else
                {
                    m.dirty = true;
                }
            });
    }
}