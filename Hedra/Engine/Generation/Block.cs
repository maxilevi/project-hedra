/*
 * Author: Zaphyk
 * Date: 31/01/2016
 * Time: 07:40 p.m.
 *
 */
using System.Runtime.InteropServices;
using Hedra.BiomeSystem;
using Hedra.Engine.BiomeSystem;
using OpenTK;

namespace Hedra.Engine.Generation
{
    [StructLayout(LayoutKind.Explicit, Size=4)]
    public struct Block
    {
        /*
         * We expose the variables instead of using properties,
         * because the compiler translates properties to method calls
         * which have a slight but noticeably overhead when it's called
         * more than a million times.
         */
        [FieldOffset(0)] public BlockType Type;
        [FieldOffset(1)] public bool Noise3D;
        [FieldOffset(2)] public Half Density;

        public Block(BlockType Type, Half Density = default(Half))
        {
            this.Type = Type;
            this.Noise3D = false;
            this.Density = Density;
        }

        public Vector4 GetColor(RegionColor Region)
        {
            return Block.GetColor(Type, Region);
        }

        public static Vector4 GetColor(BlockType Type, RegionColor Region)
        {
            return BlockUtils.GetColor(Type, Region);
        }
    }
}
