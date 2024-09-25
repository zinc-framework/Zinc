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
    public static DecomposedMatrix Decompose(this Matrix4x4 matrix)
    {
        DecomposedMatrix result = new DecomposedMatrix();

        // Extract translation
        result.Translation = new Vector3(matrix.M41, matrix.M42, matrix.M43);

        // Extract scale
        result.Scale = new Vector3(
            new Vector3(matrix.M11, matrix.M12, matrix.M13).Length(),
            new Vector3(matrix.M21, matrix.M22, matrix.M23).Length(),
            new Vector3(matrix.M31, matrix.M32, matrix.M33).Length()
        );

        // Extract rotation
        Matrix4x4 rotationMatrix = new Matrix4x4(
            matrix.M11 / result.Scale.X, matrix.M12 / result.Scale.X, matrix.M13 / result.Scale.X, 0,
            matrix.M21 / result.Scale.Y, matrix.M22 / result.Scale.Y, matrix.M23 / result.Scale.Y, 0,
            matrix.M31 / result.Scale.Z, matrix.M32 / result.Scale.Z, matrix.M33 / result.Scale.Z, 0,
            0, 0, 0, 1
        );
        result.Rotation = Quaternion.CreateFromRotationMatrix(rotationMatrix);

        return result;
    }

    public static Matrix4x4 RecomposeWithoutSkew(DecomposedMatrix decomposed)
    {
        return Matrix4x4.CreateScale(decomposed.Scale) *
               Matrix4x4.CreateFromQuaternion(decomposed.Rotation) *
               Matrix4x4.CreateTranslation(decomposed.Translation);
    }

    public static Position ToWorldPosition(this Matrix4x4 matrix)
    {
        return Position.FromMatrix(matrix);
    }
}