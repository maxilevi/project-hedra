using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Numerics;

namespace Hedra.Engine.Generation.ChunkSystem
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CoordinateHash3D : IEquatable<CoordinateHash3D>
    {
        public const int MaxCoordinateSizeXZ = 32;
        public const int MaxCoordinateSizeY = 64;
        private readonly ushort _bits;

        public CoordinateHash3D(Vector3 Coordinates): this((byte)Coordinates.X, (byte)Coordinates.Y, (byte)Coordinates.Z)
        {
        }
        
        public CoordinateHash3D(int X, int Y, int Z): this((byte)X, (byte)Y, (byte)Z)
        {
        }

        private CoordinateHash3D(byte X, byte Y, byte Z)
        {
#if DEBUG
            if (MaxCoordinateSizeXZ != (int)(Chunk.Width / Chunk.BlockSize) || MaxCoordinateSizeY != (int)(Chunk.Height / Chunk.BlockSize))
                throw new ArgumentOutOfRangeException($"Invalid coordinate size.");
#endif
            _bits = (ushort) (X | (Z << 5) | (Y << 10));
        }

        public Vector3 ToVector3()
        {
            return new Vector3(_bits & 31, (_bits >> 10) & 63, (_bits >> 5) & 31);
        }
        
        public bool Equals(CoordinateHash3D Other)
        {
            return _bits == Other._bits;
        }

        public override int GetHashCode() => _bits.GetHashCode();
    }
}