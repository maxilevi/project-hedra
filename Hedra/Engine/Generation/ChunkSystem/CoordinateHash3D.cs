using System;
using System.Runtime.InteropServices;
using OpenTK;

namespace Hedra.Engine.Generation.ChunkSystem
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CoordinateHash3D : IEquatable<CoordinateHash3D>
    {
        private readonly byte X;
        private readonly byte Y;
        private readonly byte Z;

        public CoordinateHash3D(byte X, byte Y, byte Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public CoordinateHash3D(Vector3 Coordinates)
        {
            this.X = (byte)Coordinates.X;
            this.Y = (byte)Coordinates.Y;
            this.Z = (byte)Coordinates.Z;
        }

        public bool Equals(CoordinateHash3D Other)
        {
            return X == Other.X && Y == Other.Y && Z == Other.Z;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                return hashCode;
            }
        }
    }
}
