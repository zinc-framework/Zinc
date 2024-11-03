using System.Numerics;

namespace Zinc;

public record struct GridComponent(Vector2 GridPivot, Vector2 CellPivot, float CellWidth = 8, float CellHeight = 8, int NumHorizonalCells = 8, int NumVerticalCells = 8) : IComponent
{
    public float GridWidth => CellWidth * NumHorizonalCells;
    public float GridHeight => CellHeight * NumVerticalCells;
    //square grid runs from 0,0 top left to 1,1 bottom right
    public void GetGridPosition(int index, out float x, out float y)
    {
        var startX = - GridWidth * GridPivot.X;
        var startY = - GridHeight * GridPivot.Y;

        index = index % (NumHorizonalCells * NumVerticalCells); //loop index
        x = startX + ((index % NumHorizonalCells) * CellWidth) + (CellWidth * CellPivot.X);
        y = startY + ((index / NumHorizonalCells) * CellHeight) + (CellHeight * CellPivot.Y);
    }
}