using System;
using System.Numerics;

namespace Hedra.Numerics
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
            var result = Quaternion.ToAxisAngle();
            return result.Xyz() * result.W * Mathf.Degree;
        }

        /// <summary>
        /// Transform an euler rotation (X,Y,Z) into a Quaternion.
        /// </summary>
        /// <param name="Euler">Euler rotation. In radians.</param>
        /// <returns>A quaternion representation of the euler angles</returns>
        public static Quaternion FromEuler(Vector3 Euler)
        {
            return Quaternion.CreateFromYawPitchRoll(Euler.Y, Euler.X, Euler.Z);
        }
    }
}
