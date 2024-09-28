using System.Numerics;

namespace Zinc;

[Component<GridComponent>]
public partial class Grid : SceneEntity
{
    private readonly Action<Entity, double>? _updateWrapper;
    public Grid(int cellWidth = 8, int cellHeight = 8,  Scene? scene = null, bool startEnabled = true, Action<Grid,double>? update = null, Anchor? parent = null, List<Anchor>? children = null)
        : base(startEnabled,scene,parent:parent,children:children)
    {
        Name = "Grid";
        CellWidth = cellWidth;
        CellHeight = cellHeight;
        // GridPivot = new Vector2(0.5f);
        GridPivot = new Vector2(0.5f);
        CellPivot = Vector2.Zero;
        if (update != null && _updateWrapper == null)
        {
            _updateWrapper = (baseEntity, dt) => update((Grid)baseEntity, dt);
            Update = _updateWrapper;
        }
    }
}