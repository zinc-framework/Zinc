using System.Numerics;

namespace Zinc.Core;

public struct DecomposedMatrix
{
    public Vector3 Translation;
    public Quaternion Rotation;
    public Vector3 Scale;
}
public static class Matrix4x4Extensions
{
    public static void Decompose(this Matrix3x2 matrix, out Vector2 translation, out float rotation, out Vector2 scale)
    {
        //note System.Numercis.Matrix3x2 is a row major matrix

        // Extract translation
        translation = new Vector2(matrix.M31, matrix.M32);

        // Extract scale
        scale = new Vector2(
            (float)Math.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12),
            (float)Math.Sqrt(matrix.M21 * matrix.M21 + matrix.M22 * matrix.M22)
        );

        // Extract rotation
        rotation = (float)Math.Atan2(matrix.M12, matrix.M11);

        // Ensure correct rotation sign and scale direction
        if (matrix.M11 * matrix.M22 - matrix.M12 * matrix.M21 < 0)
        {
            scale.Y = -scale.Y;
        }
    }
}