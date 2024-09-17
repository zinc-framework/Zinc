using System.Numerics;
namespace Zinc.Core;

public class Grid
{
    public class GridCreationParams
    {
        public Vector2 Pivot;
        public Vector2 NormalizedPivotPos;
        public Vector2 NormalizedCellOffset;
        public int CellWidth;
        public int CellHeight;
        public int Xcount;
        public int Ycount;
        public GridCreationParams(Vector2 pivot,
            Vector2 normalizedPivotPos,
            int cellWidth,
            int cellHeight,
            Vector2 normalizedCellOffset,
            int xcount,
            int ycount)
        {
            Pivot = pivot;
            NormalizedPivotPos = normalizedPivotPos;
            NormalizedCellOffset = normalizedCellOffset;
            CellWidth = cellWidth;
            CellHeight = cellHeight;
            Xcount = xcount;
            Ycount = ycount;
        }
    }
    
    public Vector2 Pivot { get; private set; }
    private Vector2 normalizedPivot;
    private float cellWidth, cellHeight;
    private int xCount, yCount;
    private Vector2 normalizedCellOffset;
    private List<Vector2> originalPoints; // To store the original points
    public List<Vector2> Points { get; private set; }

    public Grid(GridCreationParams p) : this(p.Pivot, p.NormalizedPivotPos, p.CellWidth, p.CellHeight,
        p.NormalizedCellOffset, p.Xcount, p.Ycount){}
    public Grid(Vector2 pivot, Vector2 normalizedPivot, float cellWidth, float cellHeight, Vector2 normalizedCellOffset, int xCount, int yCount)
    {
        this.Pivot = pivot;
        this.normalizedPivot = normalizedPivot;
        this.cellWidth = cellWidth;
        this.cellHeight = cellHeight;
        this.normalizedCellOffset = normalizedCellOffset;
        this.xCount = xCount;
        this.yCount = yCount;

        Points = new List<Vector2>();
        originalPoints = new List<Vector2>(); // Initialize the original points list
        GeneratePoints();
    }

    private void GeneratePoints()
    {
        Points.Clear(); // Clear current points
        originalPoints.Clear(); // Clear original points
        var startX = Pivot.X - (xCount * cellWidth * normalizedPivot.X);
        var startY = Pivot.Y - (yCount * cellHeight * normalizedPivot.Y);

        for (int y = 0; y < yCount; y++)
        {
            for (int x = 0; x < xCount; x++)
            {
                var point = new Vector2(
                    startX + x * cellWidth + cellWidth * normalizedCellOffset.X,
                    startY + y * cellHeight + cellHeight * normalizedCellOffset.Y);

                Points.Add(point);
                originalPoints.Add(point); // Also add to original points
            }
        }
    }

    public void TransformGrid(float rotation, float scaleX, float scaleY, Vector2? translation = null)
    {
        Console.WriteLine($"{Points[0].X},{Points[0].Y}");
        for (int i = 0; i < originalPoints.Count; i++)
        {
            Points[i] = originalPoints[i].Transform(rotation, scaleX, scaleY, translation, Pivot);
        }
        Console.WriteLine($"{Points[0].X},{Points[0].Y}");
    }
    
    public void ApplyPositionsToEntites<T>(List<T> entities) where T : SceneEntity
    {
        for (int i = 0; i < entities.Count; i++)
        {
            if (i < Points.Count)
            {
                entities[i].X = (int)Points[i].X;
                entities[i].Y = (int)Points[i].Y;
            }
        }
    }

    public void ApplyPositionToEntity<T>(int i, T entity) where T : SceneEntity
    {
        if (i >= Points.Count)
        {
            Console.WriteLine("grid index out of bounds");
            return;
        }
        entity.X = (int)Points[i].X;
        entity.Y = (int)Points[i].Y;
    }
}
