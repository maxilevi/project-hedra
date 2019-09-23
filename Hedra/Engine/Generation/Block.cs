/*
 * Author: Zaphyk
 * Date: 31/01/2016
 * Time: 07:40 p.m.
 *
 */

using System;
using Hedra.BiomeSystem;
using Hedra.Core;
using OpenTK;

namespace Hedra.Engine.Generation
{
    public struct Block
    {
        public ushort _bits;

        public Block(BlockType Type, float Density = default(float))
        {
            _bits = 0;
            this.Type = Type;
            this.Density = Density;
        }

        public float Density
        {
            get => (float) ((((_bits >> 11) & 1) * 2 - 1) * ((_bits & 0x7FF) * 0.01f));
            set
            {
                var sign = value < 0 ? 0 : 1;
                var significant = Math.Min((int) Math.Abs(value * 100), 2047);
#if DEBUG
                if(significant >= 2048) throw new ArgumentOutOfRangeException($"Significant should be less than 2048 but its '{significant}'");
                if(sign != 0 && sign != 1) throw new ArgumentOutOfRangeException($"Sign should be either 1 or 0 but its '{sign}'");
#endif
                _bits = (ushort) (((_bits >> 11) << 11) | ((sign << 11) | significant));
            }
        }

        public BlockType Type
        {
            get => (BlockType) (_bits >> 12);
            set
            {
                var type = (byte) value;
#if DEBUG
                if(type >= 16) throw new ArgumentOutOfRangeException($"BlockType should be less than 16 but its '{type}'");
#endif
                _bits = (ushort)(((ushort) (_bits << 4) >> 4) | (type << 12));
            }
        }
        
        public Vector4 GetColor(RegionColor Region)
        {
            return GetColor(Type, Region);
        }

        public static Vector4 GetColor(BlockType Type, RegionColor Region)
        {
            return BlockUtils.GetColor(Type, Region);
        }
        
        public static bool Noise3D => false;
    }

    /*
    * BlockType  Sign   Significant
    *   0000      0     0000000000
    */
    public struct HalfBlock
    {
        private readonly ushort _bits;
        
        public HalfBlock(BlockType Type, Half Density)
        {
            var type = (byte) Type;
            var sign = Density < 0 ? 0 : 1;
            var significant = Math.Min((int) Math.Abs(Density * 100), 2047);
            if(type >= 16) throw new ArgumentOutOfRangeException($"BlockType should be less than 16 but its '{type}'");
            if(significant >= 2048) throw new ArgumentOutOfRangeException($"Significant should be less than 2048 but its '{significant}'");
            if(sign != 0 && sign != 1) throw new ArgumentOutOfRangeException($"Sign should be either 1 or 0 but its '{sign}'");
            _bits = (ushort) ((type << 12) | (sign << 11) | significant);
        }

        
        public Half Density => new Half((((_bits >> 11) & 1) * 2 - 1) * ((_bits & 0x7FF) * 0.01f));
        public BlockType Type => (BlockType) (_bits >> 12);
    }
}
