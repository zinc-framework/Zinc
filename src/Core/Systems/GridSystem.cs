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

public class GridSystem : DSystem, IPreUpdateSystem
{
    QueryDescription grids = new QueryDescription().WithAll<ActiveState,EntityID,GridComponent>(); 
    public void PreUpdate(double dt)
    {
        Engine.ECSWorld.Query(in grids, (Arch.Core.Entity e, ref ActiveState a, ref EntityID managedID, ref GridComponent gc) => {
            if(!a.Active){return;}
            var grid = Engine.GetEntity(managedID.ID) as Grid;
            var children = grid.GetChildren();
            // var startX = grid.X - gc.GridWidth * gc.GridPivot.X;
            // var startY = grid.Y - gc.GridHeight * gc.GridPivot.Y;
            var startX = - gc.GridWidth * gc.GridPivot.X;
            var startY = - gc.GridHeight * gc.GridPivot.Y;
            Console.WriteLine("Grid width: " + gc.GridWidth + " Grid height: " + gc.GridHeight);
            for (int i = 0; i < children.Count; i++)
            {
                grid.ECSEntity.Get<GridComponent>().GetGridPosition(i, out var x, out var y);
                children[i].X = startX + x;
                children[i].Y = startY + y;
            }
        });
    }
}
