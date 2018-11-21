/*
 * Author: Zaphyk
 * Date: 03/03/2016
 * Time: 10:34 p.m.
 *
 */

using System;
using Hedra.BiomeSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.Generation
{
    /// <summary>
    /// Description of BlockUtils.
    /// </summary>
    public static class BlockUtils
    {
        public static Vector4 GetColor(BlockType Type, RegionColor RegionColor)
        {
            if (RegionColor == null) return Colors.Red;
            switch (Type)
            {
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

                case BlockType.StonePath:
                    return RegionColor.StonePathColor;
                case BlockType.DarkStonePath:
                    return RegionColor.DarkStonePathColor;
                   
                default:
                    return Colors.Transparent;
                    throw new ArgumentOutOfRangeException($"Unkown block type {Type}");
            }
        }
    }

    public enum BlockType : byte
    {
        Air,
        Grass,
        Stone,
        Dirt,
        Water,
        Wood,
        Leaves,
        Rock,
        Path,
        Temporal,
        Seafloor,
        StonePath,
        DarkStonePath,
        MaxNums
    }
}
