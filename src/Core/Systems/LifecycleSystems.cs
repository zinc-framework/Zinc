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
    QueryDescription eventQuery = new QueryDescription().WithAll<CollisionEvent>();
    QueryDescription managedCleanupQuery = new QueryDescription().WithAll<Destroy,EntityID>();
    public void DestroyObjects()
    {
        Engine.ECSWorld.Query(in managedCleanupQuery, (Arch.Core.Entity e, ref EntityID owner) =>
        {
            Console.WriteLine("destroying " + Engine.EntityLookup[owner.ID].Name);
            Engine.EntityLookup.Remove(owner.ID);
        });
            
        
        Engine.ECSWorld.Query(in eventQuery, (Arch.Core.Entity e, ref CollisionEvent ce) =>
        {
            var e1 = Engine.EntityLookup[ce.entity1ManagedID];
            var e2 = Engine.EntityLookup[ce.entity2ManagedID];
            if (e1.StagedForDestruction || e2.StagedForDestruction)
            {
                e.Add(new Destroy());
            }
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