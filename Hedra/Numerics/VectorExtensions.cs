using System;
using System.Numerics;

namespace Hedra.Numerics
{
    public static class VectorExtensions
    {
        public static Vector2 Xz(this Vector3 Vector)
        {
            return new Vector2(Vector.X, Vector.Z);
        }

        public static Vector2 Xy(this Vector3 Vector)
        {
            return new Vector2(Vector.X, Vector.Y);
        }

        public static Vector2 Yz(this Vector3 Vector)
        {
            return new Vector2(Vector.Y, Vector.Z);
        }

        public static Vector3 Xyz(this Vector4 Vector)
        {
            return new Vector3(Vector.X, Vector.Y, Vector.Z);
        }

        public static Vector2 PerpendicularLeft(this Vector2 Vector)
        {
            return new Vector2(Vector.Y, -Vector.X);
        }

        public static Vector2 PerpendicularRight(this Vector2 Vector)
        {
            return new Vector2(-Vector.Y, Vector.X);
        }

        public static Matrix4x4 ClearTranslation(this Matrix4x4 Matrix)
        {
            return new Matrix4x4(
                Matrix.M11, Matrix.M12, Matrix.M13, Matrix.M14,
                Matrix.M21, Matrix.M22, Matrix.M23, Matrix.M24,
                Matrix.M31, Matrix.M32, Matrix.M33, Matrix.M34,
                0, 0, 0, Matrix.M44
            );
        }

        public static Matrix4x4 ClearScale(this Matrix4x4 Matrix)
        {
            if (!Matrix4x4.Decompose(Matrix, out _, out var quaternion, out var translation))
                throw new ArgumentException("Failed to decompose matrix");
            return Matrix4x4.CreateFromQuaternion(quaternion) * Matrix4x4.CreateTranslation(translation);
        }

        public static Vector3 ExtractTranslation(this Matrix4x4 Matrix)
        {
            if (!Matrix4x4.Decompose(Matrix, out _, out _, out var translation))
                return Vector3.Zero;
            return translation;
        }

        public static Vector3 ExtractScale(this Matrix4x4 Matrix)
        {
            if (!Matrix4x4.Decompose(Matrix, out var scale, out _, out _))
                return Vector3.One;
            return scale;
        }

        public static Quaternion ExtractRotation(this Matrix4x4 Matrix)
        {
            if (!Matrix4x4.Decompose(Matrix, out _, out var quaternion, out _))
                return Quaternion.Identity;
            return quaternion;
        }

        public static Vector4 Row0(this Matrix4x4 Matrix)
        {
            return new Vector4(Matrix.M11, Matrix.M12, Matrix.M13, Matrix.M14);
        }

        public static Vector4 Row1(this Matrix4x4 Matrix)
        {
            return new Vector4(Matrix.M21, Matrix.M22, Matrix.M23, Matrix.M24);
        }

        public static Vector4 Row2(this Matrix4x4 Matrix)
        {
            return new Vector4(Matrix.M31, Matrix.M32, Matrix.M33, Matrix.M34);
        }

        public static Vector4 Row3(this Matrix4x4 Matrix)
        {
            return new Vector4(Matrix.M41, Matrix.M42, Matrix.M43, Matrix.M44);
        }

        public static Vector4 Column0(this Matrix4x4 Matrix)
        {
            return new Vector4(Matrix.M11, Matrix.M21, Matrix.M31, Matrix.M41);
        }

        public static Vector4 Column1(this Matrix4x4 Matrix)
        {
            return new Vector4(Matrix.M12, Matrix.M22, Matrix.M32, Matrix.M42);
        }

        public static Vector4 Column2(this Matrix4x4 Matrix)
        {
            return new Vector4(Matrix.M13, Matrix.M23, Matrix.M33, Matrix.M43);
        }

        public static Vector4 Column3(this Matrix4x4 Matrix)
        {
            return new Vector4(Matrix.M14, Matrix.M24, Matrix.M34, Matrix.M44);
        }

        public static Matrix4x4 Transposed(this Matrix4x4 Matrix)
        {
            return Matrix4x4.Transpose(Matrix);
        }

        public static Matrix4x4 Inverted(this Matrix4x4 Matrix)
        {
            if (!Matrix4x4.Invert(Matrix, out var result))
                return Matrix;
            return result;
        }

        public static Vector4 ToAxisAngle(this Quaternion Quaternion)
        {
            var q = Quaternion;
            if (Math.Abs(q.W) > 1.0f) q = Quaternion.Normalized();
            var result = new Vector4
            {
                W = 2.0f * (float)Math.Acos(q.W) // angle
            };

            var den = (float)Math.Sqrt(1.0 - q.W * q.W);
            if (den > 0.0001f)
            {
                result.X = q.X / den;
                result.Y = q.Y / den;
                result.Z = q.Z / den;
            }
            else
            {
                // This occurs when the angle is zero.
                // Not a problem: just set an arbitrary normalized axis.
                result = new Vector4(1, 0, 0, result.W);
            }

            return result;
        }

        public static Quaternion NormalizedFast(this Quaternion Quaternion)
        {
            float x = Quaternion.X, y = Quaternion.Y, z = Quaternion.Z, w = Quaternion.W;
            var n = 1f / Mathf.FastSqrt(x * x + y * y + z * z + w * w);
            return Quaternion * n;
        }

        public static Quaternion Normalized(this Quaternion Quaternion)
        {
            return Quaternion != default ? Quaternion.Normalize(Quaternion) : Quaternion;
        }

        #region Vector3

        public static float Length(this Vector3 Vector)
        {
            return Vector.Length();
        }

        public static float LengthFast(this Vector3 Vector)
        {
            return Vector.Length();
        }

        public static float LengthSquared(this Vector3 Vector)
        {
            return Vector.LengthSquared();
        }

        public static Vector3 Normalized(this Vector3 Vector)
        {
            return Vector != Vector3.Zero ? Vector3.Normalize(Vector) : Vector3.Zero;
        }

        public static Vector3 NormalizedFast(this Vector3 Vector)
        {
            return Vector != Vector3.Zero ? Vector3.Normalize(Vector) : Vector3.Zero;
        }

        #endregion

        #region Vector2

        public static float Length(this Vector2 Vector)
        {
            return Vector.Length();
        }

        public static float LengthFast(this Vector2 Vector)
        {
            return Vector.Length();
        }

        public static float LengthSquared(this Vector2 Vector)
        {
            return Vector.LengthSquared();
        }

        public static Vector2 Normalized(this Vector2 Vector)
        {
            return Vector != Vector2.Zero ? Vector2.Normalize(Vector) : Vector2.Zero;
        }

        public static Vector2 NormalizedFast(this Vector2 Vector)
        {
            return Vector != Vector2.Zero ? Vector2.Normalize(Vector) : Vector2.Zero;
        }

        #endregion

        #region Vector4

        public static float Length(this Vector4 Vector)
        {
            return Vector.Length();
        }

        public static float LengthFast(this Vector4 Vector)
        {
            return Vector.Length();
        }

        public static float LengthSquared(this Vector4 Vector)
        {
            return Vector.LengthSquared();
        }

        public static Vector4 Normalized(this Vector4 Vector)
        {
            return Vector != Vector4.Zero ? Vector4.Normalize(Vector) : Vector4.Zero;
        }

        public static Vector4 NormalizedFast(this Vector4 Vector)
        {
            return Vector != Vector4.Zero ? Vector4.Normalize(Vector) : Vector4.Zero;
        }

        #endregion
    }
}