using System;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.Pathfinding
{
    /// <summary>
    /// A 2D offset structure. You can use an array of offsets to represent the movement pattern
    /// of your agent, for example an offset of (-1, 0) means your character is able
    /// to move a single cell to the left <see cref="MovementPatterns"/> for some predefined
    /// options.
    /// </summary>
    public struct Offset : IEquatable<Offset>
    {
        private const float DiagonalCost = 1.4142135623730950488016887242097f; // sqrt(2)
        private const float LateralCost = 1.0f;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="X">x-movement offset</param>
        /// <param name="Y">y-movement offset</param>
        public Offset(int X, int Y)
        {
            if (X < -1 || X > 1)
                throw new ArgumentOutOfRangeException(nameof(X), $"Parameter {nameof(X)} cannot have a magnitude larger than one");

            if (Y < -1 || Y > 1)
                throw new ArgumentOutOfRangeException(nameof(Y), $"Parameter {nameof(Y)} cannot have a magnitude larger than one");

            if (X == 0 && Y == 0)
                throw new ArgumentException(nameof(Y), $"Paramters {nameof(X)} and {nameof(Y)} cannot both be zero");

            this.X = X;
            this.Y = Y;

            // Penalize diagonal movement
            this.Cost = (X != 0 && Y != 0) ? DiagonalCost : LateralCost;                                   
        }

        /// <summary>
        /// X-position
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Y-position
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Relative cost of adding this offset to a position, either 1 for lateral movement, or sqrt(2) for diagonal movement
        /// </summary>
        public float Cost { get; }

        public override string ToString() => $"Offset: ({this.X}, {this.Y})";
        
        public bool Equals(Offset Other)
        {
            return this.X == Other.X && this.Y == Other.Y;
        }

        public override bool Equals(object Obj)
        {
            if (ReferenceEquals(null, Obj))
                return false;

            return Obj is Offset && Equals((Offset)Obj);
        }

        public static bool operator ==(Offset A, Offset B)
        {
            return A.Equals(B);
        }

        public static bool operator !=(Offset A, Offset B)
        {
            return !A.Equals(B);
        }      

        public static Vector2 operator +(Offset A, Vector2 B)
        {
            return new Vector2(A.X + B.X, A.Y + B.Y);
        }

        public static Vector2 operator -(Offset A, Vector2 B)
        {
            return new Vector2(A.X - B.X, A.Y - B.Y);
        }

        public static Vector2 operator +(Vector2 A, Offset B)
        {
            return new Vector2(A.X + B.X, A.Y + B.Y);
        }

        public static Vector2 operator -(Vector2 A, Offset B)
        {
            return new Vector2(A.X - B.X, A.Y - B.Y);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.X * 397) ^ this.Y;
            }
        }
    }
}
