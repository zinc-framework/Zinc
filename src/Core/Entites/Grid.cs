using System.Numerics;
using Arch.Core.Extensions;

namespace Zinc;

[Component<GridComponent>]
public partial class Grid : SceneEntity
{
    private readonly Action<Entity, double>? _updateWrapper;
    public Grid(float cellWidth = 8, float cellHeight = 8, int numHorizontalCells = 4, int numVerticalCells = 4, Scene? scene = null, bool startEnabled = true, Action<Grid,double>? update = null, Anchor? parent = null, List<Anchor>? children = null)
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

    /// <summary>
    /// This function updates grid child positions immediately instead of waiting on the system to update them.
    /// </summary>
    public void PushGridPositions()
    {
        var children = GetChildren();
        for (int i = 0; i < children.Count; i++)
        {
            ECSEntity.Get<GridComponent>().GetGridPosition(i, out var x, out var y);
            children[i].X = x;
            children[i].Y = y;
            switch (RotationBehavior)
            {
                case GridComponent.ChildRotationBehavior.Match:
                    children[i].Rotation = Rotation;
                    break;
                case GridComponent.ChildRotationBehavior.Invert:
                    children[i].Rotation = -Rotation;
                    break;
            }
        }
    }

    public void GetLocalGridPosition(int index, out float x, out float y) => ECSEntity.Get<GridComponent>().GetGridPosition(index, out x, out y);
}