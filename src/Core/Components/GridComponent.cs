using System.Numerics;

namespace Zinc;

public record struct GridComponent(Vector2 GridPivot, Vector2 CellPivot, int CellWidth = 8, int CellHeight = 8, int NumHorizonalCells = 8, int NumVerticalCells = 8) : IComponent
{
    public int GridWidth => CellWidth * NumHorizonalCells;
    public int GridHeight => CellHeight * NumVerticalCells;
    //square grid runs from 0,0 top left to 1,1 bottom right
    public void GetGridPosition(int index, out float x, out float y)
    {
        index = index % (NumHorizonalCells * NumVerticalCells - 1); //loop index
        float localX = ((index % NumHorizonalCells) * CellWidth) + (CellWidth * CellPivot.X);
        float localY = ((index / NumHorizonalCells) * CellHeight) + (CellHeight * CellPivot.Y);
        x = localX - GridWidth * GridPivot.X;
        y = localY - GridHeight * GridPivot.Y;
        // return new Vector2(localX - GridWidth * GridPivot.X, localY - GridHeight * GridPivot.Y);
    }
}