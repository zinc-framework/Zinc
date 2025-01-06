using System.Numerics;

namespace Zinc;
[Arch.AOT.SourceGenerator.Component]
public record struct GridComponent(Vector2 GridPivot, Vector2 CellPivot, float CellWidth = 8, float CellHeight = 8, int NumHorizonalCells = 8, int NumVerticalCells = 8, GridComponent.ChildRotationBehavior RotationBehavior = GridComponent.ChildRotationBehavior.Match) : IComponent
{
    public enum ChildRotationBehavior
    {
        //when the parent grid is rotated, the children are rotated in the same direction as well so the children dont appear to rotate indpendently
        Match,
        //when the parents grid is rotated, the children are rotated in the opposite rotation such that they maintain their local rotation and dont appear to rotate independently
        Invert,
        //when the grid rotates, the children's rotation is not updated
        None

    }
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