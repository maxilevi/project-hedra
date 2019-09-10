#region License
/*
MIT License

Copyright(c) 2017 Mattias Edlund

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

using System;
using System.Runtime.CompilerServices;
using OpenTK;

namespace UnityMeshSimplifier
{
    /// <summary>
    /// A double precision 3D vector.
    /// </summary>
    public struct Vector3d : IEquatable<Vector3d>
    {
        #region Static Read-Only
        /// <summary>
        /// The zero vector.
        /// </summary>
        public static readonly Vector3d zero = new Vector3d(0, 0, 0);
        #endregion

        #region Consts
        /// <summary>
        /// The vector epsilon.
        /// </summary>
        public const double Epsilon = double.Epsilon;
        #endregion

        #region Fields
        /// <summary>
        /// The x component.
        /// </summary>
        public double X;
        /// <summary>
        /// The y component.
        /// </summary>
        public double Y;
        /// <summary>
        /// The z component.
        /// </summary>
        public double Z;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the magnitude of this vector.
        /// </summary>
        public double Magnitude
        {
            [MethodImpl(256)]
            get { return System.Math.Sqrt(X * X + Y * Y + Z * Z); }
        }

        /// <summary>
        /// Gets the squared magnitude of this vector.
        /// </summary>
        public double MagnitudeSqr
        {
            [MethodImpl(256)]
            get { return (X * X + Y * Y + Z * Z); }
        }

        /// <summary>
        /// Gets a normalized vector from this vector.
        /// </summary>
        public Vector3d Normalized
        {
            [MethodImpl(256)]
            get
            {
                Vector3d result;
                Normalize(ref this, out result);
                return result;
            }
        }

        /// <summary>
        /// Gets or sets a specific component by index in this vector.
        /// </summary>
        /// <param name="index">The component index.</param>
        public double this[int index]
        {
            [MethodImpl(256)]
            get
            {
                switch (index)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    case 2:
                        return Z;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3d index!");
                }
            }
            [MethodImpl(256)]
            set
            {
                switch (index)
                {
                    case 0:
                        X = value;
                        break;
                    case 1:
                        Y = value;
                        break;
                    case 2:
                        Z = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3d index!");
                }
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new vector with one value for all components.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(256)]
        public Vector3d(double value)
        {
            this.X = value;
            this.Y = value;
            this.Z = value;
        }

        /// <summary>
        /// Creates a new vector.
        /// </summary>
        /// <param name="x">The x value.</param>
        /// <param name="y">The y value.</param>
        /// <param name="z">The z value.</param>
        [MethodImpl(256)]
        public Vector3d(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// Creates a new vector from a single precision vector.
        /// </summary>
        /// <param name="vector">The single precision vector.</param>
        [MethodImpl(256)]
        public Vector3d(Vector3 vector)
        {
            this.X = vector.X;
            this.Y = vector.Y;
            this.Z = vector.Z;
        }
        #endregion

        #region Operators
        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>The resulting vector.</returns>
        [MethodImpl(256)]
        public static Vector3d operator +(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>The resulting vector.</returns>
        [MethodImpl(256)]
        public static Vector3d operator -(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        /// <summary>
        /// Scales the vector uniformly.
        /// </summary>
        /// <param name="a">The vector.</param>
        /// <param name="d">The scaling value.</param>
        /// <returns>The resulting vector.</returns>
        [MethodImpl(256)]
        public static Vector3d operator *(Vector3d a, double d)
        {
            return new Vector3d(a.X * d, a.Y * d, a.Z * d);
        }

        /// <summary>
        /// Scales the vector uniformly.
        /// </summary>
        /// <param name="d">The scaling vlaue.</param>
        /// <param name="a">The vector.</param>
        /// <returns>The resulting vector.</returns>
        [MethodImpl(256)]
        public static Vector3d operator *(double d, Vector3d a)
        {
            return new Vector3d(a.X * d, a.Y * d, a.Z * d);
        }

        /// <summary>
        /// Divides the vector with a float.
        /// </summary>
        /// <param name="a">The vector.</param>
        /// <param name="d">The dividing float value.</param>
        /// <returns>The resulting vector.</returns>
        [MethodImpl(256)]
        public static Vector3d operator /(Vector3d a, double d)
        {
            return new Vector3d(a.X / d, a.Y / d, a.Z / d);
        }

        /// <summary>
        /// Subtracts the vector from a zero vector.
        /// </summary>
        /// <param name="a">The vector.</param>
        /// <returns>The resulting vector.</returns>
        [MethodImpl(256)]
        public static Vector3d operator -(Vector3d a)
        {
            return new Vector3d(-a.X, -a.Y, -a.Z);
        }

        /// <summary>
        /// Returns if two vectors equals eachother.
        /// </summary>
        /// <param name="lhs">The left hand side vector.</param>
        /// <param name="rhs">The right hand side vector.</param>
        /// <returns>If equals.</returns>
        [MethodImpl(256)]
        public static bool operator ==(Vector3d lhs, Vector3d rhs)
        {
            return (lhs - rhs).MagnitudeSqr < Epsilon;
        }

        /// <summary>
        /// Returns if two vectors don't equal eachother.
        /// </summary>
        /// <param name="lhs">The left hand side vector.</param>
        /// <param name="rhs">The right hand side vector.</param>
        /// <returns>If not equals.</returns>
        [MethodImpl(256)]
        public static bool operator !=(Vector3d lhs, Vector3d rhs)
        {
            return (lhs - rhs).MagnitudeSqr >= Epsilon;
        }

        /// <summary>
        /// Implicitly converts from a single-precision vector into a double-precision vector.
        /// </summary>
        /// <param name="v">The single-precision vector.</param>
        [MethodImpl(256)]
        public static implicit operator Vector3d(Vector3 v)
        {
            return new Vector3d(v.X, v.Y, v.Z);
        }

        /// <summary>
        /// Implicitly converts from a double-precision vector into a single-precision vector.
        /// </summary>
        /// <param name="v">The double-precision vector.</param>
        [MethodImpl(256)]
        public static explicit operator Vector3(Vector3d v)
        {
            return new Vector3((float)v.X, (float)v.Y, (float)v.Z);
        }
        #endregion

        #region Public Methods
        #region Instance
        /// <summary>
        /// Set x, y and z components of an existing vector.
        /// </summary>
        /// <param name="x">The x value.</param>
        /// <param name="y">The y value.</param>
        /// <param name="z">The z value.</param>
        [MethodImpl(256)]
        public void Set(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// Multiplies with another vector component-wise.
        /// </summary>
        /// <param name="scale">The vector to multiply with.</param>
        [MethodImpl(256)]
        public void Scale(ref Vector3d scale)
        {
            X *= scale.X;
            Y *= scale.Y;
            Z *= scale.Z;
        }

        /// <summary>
        /// Normalizes this vector.
        /// </summary>
        [MethodImpl(256)]
        public void Normalize()
        {
            double mag = this.Magnitude;
            if (mag > Epsilon)
            {
                X /= mag;
                Y /= mag;
                Z /= mag;
            }
            else
            {
                X = Y = Z = 0;
            }
        }

        /// <summary>
        /// Clamps this vector between a specific range.
        /// </summary>
        /// <param name="min">The minimum component value.</param>
        /// <param name="max">The maximum component value.</param>
        [MethodImpl(256)]
        public void Clamp(double min, double max)
        {
            if (X < min) X = min;
            else if (X > max) X = max;

            if (Y < min) Y = min;
            else if (Y > max) Y = max;

            if (Z < min) Z = min;
            else if (Z > max) Z = max;
        }
        #endregion

        #region Object
        /// <summary>
        /// Returns a hash code for this vector.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() << 2 ^ Z.GetHashCode() >> 2;
        }

        /// <summary>
        /// Returns if this vector is equal to another one.
        /// </summary>
        /// <param name="other">The other vector to compare to.</param>
        /// <returns>If equals.</returns>
        public override bool Equals(object other)
        {
            if (!(other is Vector3d))
            {
                return false;
            }
            Vector3d vector = (Vector3d)other;
            return (X == vector.X && Y == vector.Y && Z == vector.Z);
        }

        /// <summary>
        /// Returns if this vector is equal to another one.
        /// </summary>
        /// <param name="other">The other vector to compare to.</param>
        /// <returns>If equals.</returns>
        public bool Equals(Vector3d other)
        {
            return (X == other.X && Y == other.Y && Z == other.Z);
        }

        /// <summary>
        /// Returns a nicely formatted string for this vector.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return string.Format("({0:F1}, {1:F1}, {2:F1})", X, Y, Z);
        }

        /// <summary>
        /// Returns a nicely formatted string for this vector.
        /// </summary>
        /// <param name="format">The float format.</param>
        /// <returns>The string.</returns>
        public string ToString(string format)
        {
            return string.Format("({0}, {1}, {2})", X.ToString(format), Y.ToString(format), Z.ToString(format));
        }
        #endregion

        #region Static
        /// <summary>
        /// Dot Product of two vectors.
        /// </summary>
        /// <param name="lhs">The left hand side vector.</param>
        /// <param name="rhs">The right hand side vector.</param>
        [MethodImpl(256)]
        public static double Dot(ref Vector3d lhs, ref Vector3d rhs)
        {
            return lhs.X * rhs.X + lhs.Y * rhs.Y + lhs.Z * rhs.Z;
        }

        /// <summary>
        /// Cross Product of two vectors.
        /// </summary>
        /// <param name="lhs">The left hand side vector.</param>
        /// <param name="rhs">The right hand side vector.</param>
        /// <param name="result">The resulting vector.</param>
        [MethodImpl(256)]
        public static void Cross(ref Vector3d lhs, ref Vector3d rhs, out Vector3d result)
        {
            result = new Vector3d(lhs.Y * rhs.Z - lhs.Z * rhs.Y, lhs.Z * rhs.X - lhs.X * rhs.Z, lhs.X * rhs.Y - lhs.Y * rhs.X);
        }

        /// <summary>
        /// Calculates the angle between two vectors.
        /// </summary>
        /// <param name="from">The from vector.</param>
        /// <param name="to">The to vector.</param>
        /// <returns>The angle.</returns>
        [MethodImpl(256)]
        public static double Angle(ref Vector3d from, ref Vector3d to)
        {
            Vector3d fromNormalized = from.Normalized;
            Vector3d toNormalized = to.Normalized;
            return System.Math.Acos(MathHelper.Clamp(Vector3d.Dot(ref fromNormalized, ref toNormalized), -1.0, 1.0)) * MathHelper.Rad2Degd;
        }

        /// <summary>
        /// Performs a linear interpolation between two vectors.
        /// </summary>
        /// <param name="a">The vector to interpolate from.</param>
        /// <param name="b">The vector to interpolate to.</param>
        /// <param name="t">The time fraction.</param>
        /// <param name="result">The resulting vector.</param>
        [MethodImpl(256)]
        public static void Lerp(ref Vector3d a, ref Vector3d b, double t, out Vector3d result)
        {
            result = new Vector3d(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t, a.Z + (b.Z - a.Z) * t);
        }

        /// <summary>
        /// Multiplies two vectors component-wise.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <param name="result">The resulting vector.</param>
        [MethodImpl(256)]
        public static void Scale(ref Vector3d a, ref Vector3d b, out Vector3d result)
        {
            result = new Vector3d(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        /// <summary>
        /// Normalizes a vector.
        /// </summary>
        /// <param name="value">The vector to normalize.</param>
        /// <param name="result">The resulting normalized vector.</param>
        [MethodImpl(256)]
        public static void Normalize(ref Vector3d value, out Vector3d result)
        {
            double mag = value.Magnitude;
            if (mag > Epsilon)
            {
                result = new Vector3d(value.X / mag, value.Y / mag, value.Z / mag);
            }
            else
            {
                result = Vector3d.zero;
            }
        }
        #endregion
        #endregion
    }
}