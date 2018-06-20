/*
 * Author: Zaphyk
 * Date: 31/01/2016
 * Time: 07:40 p.m.
 *
 */
using System.Runtime.InteropServices;
using Hedra.Engine.BiomeSystem;
using OpenTK;

namespace Hedra.Engine.Generation
{
	[StructLayout(LayoutKind.Explicit, Size=6)]
	public struct Block
	{
	    [FieldOffset(0)] private BlockType _type;
		[FieldOffset(1)] private bool _noise3D;
	    [FieldOffset(2)] private float _density;

        public Block(BlockType Type, float Density = default(float))
        {
			this._type = Type;
			this._noise3D = false;
            this._density = Density;
        }

	    public BlockType Type
	    {
	        get => _type;
	        set => _type = value;
	    }

	    public bool Noise3D
	    {
	        get => _noise3D;
	        set => _noise3D = value;
	    }

	    public float Density
	    {
	        get => _density;
	        set => _density = value;
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
