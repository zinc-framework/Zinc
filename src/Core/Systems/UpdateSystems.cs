using Arch.Core;
using Arch.Core.Extensions;

namespace Zinc;
public class EntityUpdateSystem : DSystem, IUpdateSystem
{
    QueryDescription query = new QueryDescription().WithAll<ActiveState,EntityID,UpdateListener>();
    public void Update(double dt)
    {
        Engine.ECSWorld.Query(in query, (Arch.Core.Entity e, ref ActiveState a, ref EntityID id, ref UpdateListener u) => {
            if(!a.Active){return;}
            u.Update?.Invoke(Engine.GetEntity(id.ID),dt);
        });
    }
}

public class SceneUpdateSystem : DSystem, IUpdateSystem
{
    QueryDescription query = new QueryDescription().WithAll<ActiveState,EntityID,SceneComponent>();
    public void Update(double dt)
    {
        Engine.ECSWorld.Query(in query, (Arch.Core.Entity e, ref ActiveState a, ref EntityID id) => {
            if(!a.Active){return;}
            if (Engine.MountedScenes.ContainsKey(id.ID) && Engine.SceneLookup[id.ID].Status == SceneActiveStatus.Active)
            {
                Engine.SceneLookup[id.ID].Update(dt);
            }
        });
    }
}

public class TemporaryObjectSystem : DSystem, IUpdateSystem
{
    QueryDescription query = new QueryDescription().WithAll<ActiveState,EntityID,TemporaryComponent>();
    public void Update(double dt)
    {
        Engine.ECSWorld.Query(in query, (Arch.Core.Entity e, ref ActiveState a, ref EntityID id, ref TemporaryComponent tc) => {
            tc.CurrentLife += (float)dt;
            if(tc.CurrentLife >= tc.Lifetime && Engine.TryGetEntity(id.ID, out var entity) && !entity.StagedForDestruction)
            {
                entity.Destroy();
            }
        });
    }
}