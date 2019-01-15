/*
 * Author: Zaphyk
 * Date: 31/01/2016
 * Time: 07:40 p.m.
 *
 */

using System;
using Hedra.BiomeSystem;
using OpenTK;

namespace Hedra.Engine.Generation
{
    public struct Block
    {
        /*
         * We expose the variables instead of using properties,
         * because the compiler translates properties to method calls
         * which have a slight but noticeably overhead when it's called
         * more than a million times.
         */
        private HalfBlock _typeAndDensity;

        public Block(BlockType Type, Half Density = default(Half))
        {
            _typeAndDensity = new HalfBlock(Type, Density);
        }

        public Half Density
        {
            get => _typeAndDensity.Density;
            set => _typeAndDensity = new HalfBlock(Type, value);
        }

        public BlockType Type
        {
            get => _typeAndDensity.Type;
            set => _typeAndDensity = new HalfBlock(value, Density);
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
