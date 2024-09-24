using System.Numerics;

namespace Zinc;

public interface IComponent {}

public record struct Position(float X = 0, float Y = 0, float ScaleX = 1, float ScaleY = 1, float Rotation = 0f) : IComponent
{
    public static implicit operator Matrix4x4(Position p)
    {
        float normalizedRotation = p.Rotation % (2 * MathF.PI);
        return Matrix4x4.CreateScale(p.ScaleX, p.ScaleY, 1) *
               Matrix4x4.CreateRotationZ(normalizedRotation) *
               Matrix4x4.CreateTranslation(p.X, p.Y, 0);
    }
    
    public static implicit operator Vector2(Position p) =>
        new Vector2(p.X,p.Y); 

    public static Position FromMatrix(Matrix4x4 matrix)
    {
        // Extract position
        float x = matrix.M41;
        float y = matrix.M42;

        // Extract scale
        float scaleX = MathF.Sqrt(matrix.M11 * matrix.M11 + matrix.M21 * matrix.M21);
        float scaleY = MathF.Sqrt(matrix.M12 * matrix.M12 + matrix.M22 * matrix.M22);

        // Extract rotation
        float rotation = MathF.Atan2(matrix.M21, matrix.M11);

        return new Position(x, y, scaleX, scaleY, rotation);
    }
}


//note that update needs to be tied to a managed entity
//its assumed that if you are making raw ecs entities you are working with systems
public record struct UpdateListener(Action<Zinc.Entity,double> Update) : IComponent;