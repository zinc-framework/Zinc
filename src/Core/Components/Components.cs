using System.Numerics;

namespace Zinc;

public interface IComponent {}

// public record struct Position(float X = 0, float Y = 0, float ScaleX = 1, float ScaleY = 1, float Rotation = 0f) : IComponent
// {

public record struct Position : IComponent
{
    float scaleX = 1;
    public float ScaleX { get => scaleX; set => scaleX = value; }
    float scaleY = 1;
    public float ScaleY { get => scaleY; set => scaleY = value; }
    float rotation = 0;
    public float Rotation { get => rotation; set => rotation = value; }
    float x = 0;
    public float X 
    { 
        get => x; 
        set 
        {
            var deltaX = value - x;
            x = value;
            PositionUpdated?.Invoke(deltaX,0);
        }
    }

    float y = 0;
    public float Y 
    { 
        get => y; 
        set 
        {
            var deltaY = value - y;
            y = value;
            PositionUpdated?.Invoke(0,deltaY);
        }
    }

    public Position() 
    {
        x = 0;
        y = 0;
        this.ScaleX = 1;
        this.ScaleY = 1;
        this.Rotation = 0;
    }

    public Position(float X = 0, float Y = 0, float ScaleX = 1, float ScaleY = 1, float Rotation = 0f)
    {
        this.X = X;
        this.Y = Y;
        this.ScaleX = ScaleX;
        this.ScaleY = ScaleY;
        this.Rotation = Rotation;

    }

    // params are X/Y delta
    public Action<float,float> PositionUpdated = null!;

    public static implicit operator Matrix3x2(Position p)
    {
        float normalizedRotation = p.Rotation % (2 * MathF.PI);
        var trans = Matrix3x2.CreateTranslation(p.X, p.Y);
        var rot = Matrix3x2.CreateRotation(normalizedRotation);
        var scale = Matrix3x2.CreateScale(p.ScaleX, p.ScaleY);
        //3x2 is row major (not column major)
        //so we mult right to left
        return  scale * rot * trans; //working
    }

    public (Matrix3x2 rotation, Vector2 translation, Vector2 scale) GetTransform()
    {
        float normalizedRotation = Rotation % (2 * MathF.PI);
        
        var mrotation = Matrix3x2.CreateRotation(normalizedRotation);
        var translation = new Vector2(X,Y);
        var scale = new Vector2(ScaleX, ScaleY);

        return (mrotation, translation, scale);
    }
    
    public static implicit operator Vector2(Position p) =>
        new Vector2(p.X,p.Y); 
}


//note that update needs to be tied to a managed entity
//its assumed that if you are making raw ecs entities you are working with systems
public record struct UpdateListener(Action<Zinc.Entity,double> Update) : IComponent;