/*
 * Author: Zaphyk
 * Date: 03/03/2016
 * Time: 10:34 p.m.
 *
 */

using System;
using Hedra.BiomeSystem;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
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
                
                case BlockType.Seafloor:
                    return RegionColor.SeafloorColor;

                case BlockType.StonePath:
                    return RegionColor.StonePathColor;
                
                case BlockType.FarmDirt:
                    return RegionColor.DirtColor;

                case BlockType.Air:
                    return Colors.Transparent;

                default:
                    throw new ArgumentOutOfRangeException($"Unkown block type {Type}");
            }
        }
    }

    /* BlockTypes should not exceed 16 */
    public enum BlockType : byte
    {
        Air = 0,
        Grass = 1,
        Stone = 2,
        Dirt = 3,
        Water = 4,
        Path = 7,
        Seafloor = 8,
        StonePath = 9,
        FarmDirt = 10,
        None = 11,
        MaxNums
    }
}
