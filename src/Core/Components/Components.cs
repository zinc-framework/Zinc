using System.Numerics;

namespace Zinc;

public interface IComponent {}

public record struct Position(float X = 0, float Y = 0, float ScaleX = 1, float ScaleY = 1, float Rotation = 0f) : IComponent
{
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
        
        var rotation = Matrix3x2.CreateRotation(normalizedRotation);
        var translation = new Vector2(X, Y);
        var scale = new Vector2(ScaleX, ScaleY);

        return (rotation, translation, scale);
    }
    
    public static implicit operator Vector2(Position p) =>
        new Vector2(p.X,p.Y); 
}


//note that update needs to be tied to a managed entity
//its assumed that if you are making raw ecs entities you are working with systems
public record struct UpdateListener(Action<Zinc.Entity,double> Update) : IComponent;