using System.Numerics;
using Zinc.Core;

namespace Zinc;

public interface IComponent {}

public record struct Position(float X = 0, float Y = 0, float ScaleX = 1, float ScaleY = 1, float Rotation = 0f) : IComponent
{
    public static implicit operator Matrix4x4(Position p)
    {
        // float normalizedRotation = p.Rotation % (2 * MathF.PI);
        return Matrix4x4.CreateScale(p.ScaleX, p.ScaleY, 1) *
               Matrix4x4.CreateRotationZ(p.Rotation) *
               Matrix4x4.CreateTranslation(p.X, p.Y, 0);
    }
    
    public static implicit operator Vector2(Position p) =>
        new Vector2(p.X,p.Y); 

    public static Position FromMatrix(Matrix4x4 matrix)
    {
        Zinc.Core.DecomposedMatrix decomposed = matrix.Decompose();
        return new Position
        {
            X = decomposed.Translation.X,
            Y = decomposed.Translation.Y,
            ScaleX = decomposed.Scale.X,
            ScaleY = decomposed.Scale.Y,
            Rotation = QuaternionToAngle(decomposed.Rotation)
        };
    }
    private static float QuaternionToAngle(Quaternion q)
    {
        // Convert quaternion to rotation around Z-axis
        float sinr_cosp = 2 * (q.W * q.Z + q.X * q.Y);
        float cosr_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
        return (float)Math.Atan2(sinr_cosp, cosr_cosp);
    }

    public static Position operator +(Position a, Position b)
    {
        return new Position
        {
            X = a.X + b.X,
            Y = a.Y + b.Y,
            ScaleX = a.ScaleX * b.ScaleX,
            ScaleY = a.ScaleY * b.ScaleY,
            Rotation = a.Rotation + b.Rotation
        };
    }
}


//note that update needs to be tied to a managed entity
//its assumed that if you are making raw ecs entities you are working with systems
public record struct UpdateListener(Action<Zinc.Entity,double> Update) : IComponent;