using System;
using System.Runtime.InteropServices;
using OpenTK;

namespace Hedra.Engine.Generation.ChunkSystem
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CoordinateHash2D : IEquatable<CoordinateHash2D>
    {
        private readonly byte X;
        private readonly byte Y;

        public CoordinateHash2D(int X, int Y)
        {
            this.X = (byte) X;
            this.Y = (byte) Y;
        }

        public CoordinateHash2D(byte X, byte Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public CoordinateHash2D(Vector2 Coordinates)
        {
            this.X = (byte)Coordinates.X;
            this.Y = (byte)Coordinates.Y;
        }

        public bool Equals(CoordinateHash2D Other)
        {
            return X == Other.X && Y == Other.Y;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                return hashCode;
            }
        }
    }
}
