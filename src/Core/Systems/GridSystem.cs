using Arch.Core;
using Arch.Core.Extensions;

namespace Zinc;

public class GridSystem : DSystem, IPreUpdateSystem
{
    QueryDescription grids = new QueryDescription().WithAll<ActiveState,EntityID,GridComponent>(); 
    public void PreUpdate(double dt)
    {
        Engine.ECSWorld.Query(in grids, (Arch.Core.Entity e, ref ActiveState a, ref EntityID managedID, ref GridComponent gc) => {
            if(!a.Active){return;}
            var grid = Engine.GetEntity(managedID.ID) as Grid;
            var children = grid.GetChildren();
            for (int i = 0; i < children.Count; i++)
            {
                grid.ECSEntity.Get<GridComponent>().GetGridPosition(i, out var x, out var y);
                children[i].LocalX = x;
                children[i].LocalY = y;
                children[i].Rotation = grid.Rotation;
            }
        });
    }
}
