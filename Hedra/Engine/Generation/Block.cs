/*
 * Author: Zaphyk
 * Date: 31/01/2016
 * Time: 07:40 p.m.
 *
 */

using System.Drawing;
using System.Runtime.InteropServices;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Rendering;
using OpenTK;


namespace Hedra.Engine.Generation
{
	[StructLayout(LayoutKind.Explicit, Size=6)]
	public struct Block
	{
		[FieldOffset(0)]
		public BlockType Type;
		[FieldOffset(1)]
		public bool Noise3D;
	    [FieldOffset(2)]
	    public float Density;

        public Block(BlockType Type, float Density = 0){
			this.Type = Type;
			this.Density = Density;
			this.Noise3D = false;
		}
		
		public Vector4 GetColor(RegionColor RegionColor)
		{
		    if (RegionColor == null) return Colors.Red;
			switch(Type){
				case BlockType.Grass:
					return Colors.Blue;//Give back the default color
				case BlockType.Stone:
					return RegionColor.StoneColor;
				case BlockType.Path:
					return RegionColor.PathColor;
				case BlockType.Dirt:
					return RegionColor.DirtColor;
				case BlockType.Water:
					return RegionColor.WaterColor;
				case BlockType.Wood:
					return RegionColor.WoodColor;
				case BlockType.Leaves:
					return RegionColor.LeavesColor;
				case BlockType.Seafloor:
					return RegionColor.SeafloorColor;
				case BlockType.Sand:
					return RegionColor.SandColor;
				case BlockType.City:
					return Colors.FromHtml("#bcb9b4");
				default:
					return Colors.Transparent;
			}
		}
		
		public static Vector4 GetColor(BlockType Type, RegionColor R){
		    var b = new Block
		    {
		        Type = Type
		    };
		    return b.GetColor(R);
		}
	}
	
	public enum BlockType : byte{
		Air,
		Grass,
		Stone,
		Dirt,
		Water,
		Wood,
		Leaves,
		Rock,
		Path,
		Cube,
		Temporal,
		Seafloor,
		Sand,
		City,
		MaxNums
	}
}
