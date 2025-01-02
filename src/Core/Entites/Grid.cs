using System.Numerics;
using Arch.Core.Extensions;

namespace Zinc;

[Component<GridComponent>]
public partial class Grid : SceneEntity
{
    private readonly Action<Entity, double>? _updateWrapper;
    public Grid(int cellWidth = 8, int cellHeight = 8, int numHorizontalCells = 4, int numVerticalCells = 4, Scene? scene = null, bool startEnabled = true, Action<Grid,double>? update = null, Anchor? parent = null, List<Anchor>? children = null)
        : base(startEnabled,scene,parent:parent,children:children)
    {
        Name = "Grid";
        CellWidth = cellWidth;
        CellHeight = cellHeight;
        NumHorizonalCells = numHorizontalCells;
        NumVerticalCells = numVerticalCells;
        GridPivot = new Vector2(0.5f);
        CellPivot = new Vector2(0.5f);
        if (update != null && _updateWrapper == null)
        {
            _updateWrapper = (baseEntity, dt) => update((Grid)baseEntity, dt);
            Update = _updateWrapper;
        }
    }

    public void GetLocalGridPosition(int index, out float x, out float y) => ECSEntity.Get<GridComponent>().GetGridPosition(index, out x, out y);
}