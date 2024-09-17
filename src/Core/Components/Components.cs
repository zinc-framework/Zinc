using System.Numerics;

namespace Zinc;

public interface IComponent {}


public record struct Position(float X = 0, float Y = 0, float ScaleX = 1, float ScaleY = 1, float Rotation = 0f) : IComponent
{
    public static implicit operator Matrix3x2(Position p) => 
        Matrix3x2.CreateScale(p.ScaleX,p.ScaleY) * 
        Matrix3x2.CreateRotation(p.Rotation) * 
        Matrix3x2.CreateTranslation(p.X,p.Y);
    
    public static implicit operator Vector2(Position p) =>
        new Vector2(p.X,p.Y); 
    
    //override + operator to add two positions together
    public static Position operator +(Position a, Position b) => 
        new Position(a.X + b.X, a.Y + b.Y, a.ScaleX + b.ScaleX, a.ScaleY + b.ScaleY, a.Rotation + b.Rotation);
}


//note that update needs to be tied to a managed entity
//its assumed that if you are making raw ecs entities you are working with systems
public record struct UpdateListener(Action<Zinc.Entity,double> Update) : IComponent;