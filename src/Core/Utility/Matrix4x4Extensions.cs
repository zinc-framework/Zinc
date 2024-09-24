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
    public static Position ToWorldPosition(this Matrix4x4 matrix)
    {
        var (scale, rotation, translation) = DecomposeMatrix(matrix);
        
        return new Position(
            translation.X,
            translation.Y,
            scale.X,
            scale.Y,
            QuaternionToEuler(rotation).Z
        );

        (Vector3 scale, Quaternion rotation, Vector3 translation) DecomposeMatrix(Matrix4x4 matrix)
        {
            Vector3 scale;
            Quaternion rotation;
            Vector3 translation;
            
            Matrix4x4.Decompose(matrix, out scale, out rotation, out translation);
            
            return (scale, rotation, translation);
        }

        Vector3 QuaternionToEuler(Quaternion q)
        {
            Vector3 angles;

            // roll (x-axis rotation)
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
                angles.Y = (float)Math.CopySign(Math.PI / 2, sinp); // use 90 degrees if out of range
            else
                angles.Y = (float)Math.Asin(sinp);

            // yaw (z-axis rotation)
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }
    }
}