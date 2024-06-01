using System.Numerics;
using Zinc.Internal.Cute;
using Zinc.Internal.Sokol;

namespace Zinc.Core;


public static class VectorExtensions
{
    public static c2v ToC2V (this Vector2 v) => new(){x = v.X,y = v.Y};
    public static Vector2 ToVector2(this c2v v) => new(v.x, v.y);
    public static ImVec2 ToImVec2(this Vector2 v) => new ImVec2() { x = v.X, y = v.Y };
    
    public static Vector2 Transform(
        this Vector2 v,
        float rotation, 
        float scaleX, 
        float scaleY, 
        Vector2? translation = null,
        //NOTE: PIVOT IN WORLD SPACE
        Vector2? pivot = null)
    {
        Vector2 translate = translation ?? Vector2.Zero;
        Matrix3x2 transformation;
        if (pivot != null)
        {
            // Move the pivot to the origin, apply scale and rotation, then move back
            transformation =
                Matrix3x2.CreateTranslation(-pivot.Value) * // Move pivot to origin
                Matrix3x2.CreateScale(scaleX, scaleY) * // Scale around pivot
                Matrix3x2.CreateRotation(rotation) * // Rotate around pivot
                Matrix3x2.CreateTranslation(pivot.Value) * // Move back
                Matrix3x2.CreateTranslation(translate);
        }
        else
        {
            // When there's no pivot, simply apply scaling, rotation, and then translation
            transformation =
                Matrix3x2.CreateScale(scaleX, scaleY) *
                Matrix3x2.CreateRotation(rotation) *
                Matrix3x2.CreateTranslation(translate);
        }
        return Vector2.Transform(v, transformation);
    }
    
    public static Vector2 Translate(
        this Vector2 v,
        Vector2 translation)
    {
        Matrix3x2 transformation =
            Matrix3x2.CreateTranslation(translation);
        return Vector2.Transform(v, transformation);
    }
}

// public record Vector2(float X, float Y)
// {
//     private System.Numerics.Vector2 internalVec2 = new (X, Y);
//     public static implicit operator c2v(Vector2 d) => new c2v(){x = d.X,y = d.Y};
//     public static implicit operator Vector2((float, float) tuple)
//     {
//         return new Vector2(tuple.Item1, tuple.Item2);
//     }
//     public static implicit operator Vector2((int, int) tuple)
//     {
//         return new Vector2(tuple.Item1, tuple.Item2);
//     }
//     public static implicit operator Vector2(c2v c)
//     {
//         return new Vector2(c.x, c.y);
//     }
//     public static implicit operator Vector2(System.Numerics.Vector2 c)
//     {
//         return new Vector2(c.X, c.Y);
//     }
//     
//     public static implicit operator System.Numerics.Vector2(Vector2 p)
//     {
//         return new System.Numerics.Vector2(p.X, p.Y);
//     }
//     
//     public static implicit operator ImVec2(Vector2 p)
//     {
//         return new ImVec2(){x =p.X, y=p.Y};
//     }
//     
//     public static Vector2 operator +(Vector2 a, Vector2 b)
//     {
//         return new Vector2(a.X + b.X, a.Y + b.Y);
//     }
//
//     public static Vector2 operator -(Vector2 a, Vector2 b)
//     {
//         return new Vector2(a.X - b.X, a.Y - b.Y);
//     }
//     
//     public Vector2 Transform(
//         float rotation, 
//         float scaleX, 
//         float scaleY, 
//         System.Numerics.Vector2? pivot = null)
//     {
//         System.Numerics.Vector2 pivotPoint = pivot ?? System.Numerics.Vector2.Zero;
//         Matrix3x2 transformation =
//             Matrix3x2.CreateTranslation(internalVec2) *
//             Matrix3x2.CreateRotation(rotation, pivotPoint) *
//             Matrix3x2.CreateScale(scaleX, scaleY, pivotPoint);
//         return System.Numerics.Vector2.Transform(System.Numerics.Vector2.Zero, transformation);
//     }
//     
//     /*
//      *     public ClipSpaceCoordinate ToClipSpace(PixelCoordinate pixelCoordinate, PixelCoordinate pivot)
//        {
//            int translatedX = pixelCoordinate.X - pivot.X;
//            int translatedY = pixelCoordinate.Y - pivot.Y;
//
//            float x = (translatedX * 2.0f / Engine.Width) - 1.0f;
//            float y = 1.0f - (translatedY * 2.0f / Engine.Height);
//            
//            return new(x, y);
//        }
//      */
// }