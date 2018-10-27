using OpenTK;
using System;

namespace Hedra.Engine.ComplexMath
{
    public static class QuaternionMath
    {
        /// <summary>
        /// Transform a quaternion into an equivalent euler rotation.
        /// </summary>
        /// <param name="Quaternion">Quaternion to take the angles from.</param>
        /// <returns></returns>
        public static Vector3 ToEuler(Quaternion Quaternion)
        {
            double sinr = +2.0 * (Quaternion.W * Quaternion.X + Quaternion.Y * Quaternion.Z);
            double cosr = +1.0 - 2.0 * (Quaternion.X * Quaternion.X + Quaternion.Y * Quaternion.Y);
            double sinp = +2.0 * (Quaternion.W * Quaternion.Y - Quaternion.Z * Quaternion.X);
            double siny = +2.0 * (Quaternion.W * Quaternion.Z + Quaternion.X * Quaternion.Y);
            double cosy = +1.0 - 2.0 * (Quaternion.Y * Quaternion.Y + Quaternion.Z * Quaternion.Z);

            float pitch = (float) (Math.Abs(sinp) >= 1 ? (sinp > 0 ? 1f : -1f) * Math.PI * .5f : Math.Asin(sinp));
            float yaw = (float)Math.Atan2(siny, cosy);
            float roll = (float)Math.Atan2(sinr, cosr);

            return new Vector3(pitch, yaw, roll) * Mathf.Degree;
        }

        /// <summary>
        /// Transform an euler rotation (X,Y,Z) into a Quaternion.
        /// </summary>
        /// <param name="Pitch">Provided pitch</param>
        /// <param name="Yaw">Provided yaw</param>
        /// <param name="Roll">Provided roll</param>
        /// <returns>A quaternion representation of the euler angles</returns>
        public static Quaternion FromEuler(float Pitch, float Yaw, float Roll)
        {
            return QuaternionMath.FromEuler(new Vector3(Pitch, Yaw, Roll));
        }

        /// <summary>
        /// Transform an euler rotation (X,Y,Z) into a Quaternion.
        /// </summary>
        /// <param name="Euler">Euler rotation. In radians.</param>
        /// <returns>A quaternion representation of the euler angles</returns>
        public static Quaternion FromEuler(Vector3 Euler)
        {
            var cy = (float)Math.Cos(Euler.Y * 0.5);
            var sy = (float)Math.Sin(Euler.Y * 0.5);
            var cr = (float)Math.Cos(Euler.Z * 0.5);
            var sr = (float)Math.Sin(Euler.Z * 0.5);
            var cp = (float)Math.Cos(Euler.X * 0.5);
            var sp = (float)Math.Sin(Euler.X * 0.5);

            return new Quaternion(
                cy * sr * cp - sy * cr * sp,
                cy * cr * sp + sy * sr * cp,
                sy * cr * cp - cy * sr * sp,
                cy * cr * cp + sy * sr * sp
            );
        }
    }
}
