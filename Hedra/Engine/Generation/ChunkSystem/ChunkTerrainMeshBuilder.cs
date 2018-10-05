using System;
using System.Collections.Generic;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Geometry;
using OpenTK;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class ChunkTerrainMeshBuilder
    {
        private readonly Chunk _parent;
        private readonly WaterEdgePatcher _waterPatcher;
        private readonly MeshStitcher _stitcher;
        private readonly WaterMeshStitcher _waterStitcher;
        private readonly Vector3[] _lodOffsets;
        public ChunkSparsity Sparsity { get; set; }
        public ChunkTerrainMeshBuilderHelper Helper { get; }

        public ChunkTerrainMeshBuilder(Chunk Parent)
        {
            Helper = new ChunkTerrainMeshBuilderHelper(Parent);
            _parent = Parent;
            _stitcher = new MeshStitcher();
            _waterPatcher = new WaterEdgePatcher();
            _waterStitcher = new WaterMeshStitcher(_parent);
            _lodOffsets = new[]
            {
                _parent.Position + new Vector3(Chunk.Width, 0, 0),
                _parent.Position + new Vector3(0, 0, Chunk.Width),
                _parent.Position + new Vector3(-Chunk.Width, 0, 0),
                _parent.Position + new Vector3(0, 0, -Chunk.Width)
            };
        }

        private int OffsetX => _parent.OffsetX;
        private int OffsetZ => _parent.OffsetZ;
        private int BoundsX => _parent.BoundsX;
        private int BoundsY => _parent.BoundsY;
        private int BoundsZ => _parent.BoundsZ;
        private static float BlockSize => Chunk.BlockSize;

        public ChunkMeshBuildOutput CreateTerrainMesh(Block[][][] Blocks, int Lod, RegionCache Cache)
        {
            var output = CreateTerrain(Blocks,
                (X,Y,Z) => Lod == 1 || (X != 0 && Z != 0 && X != BoundsX-Lod && Z != BoundsZ-Lod), Lod, Cache);

            if (Lod != 1)
            {
                var targetLod = 1;
                var borders = CreateTerrain(Blocks,
                    (X, Y, Z) => X < targetLod || Z < targetLod || X > BoundsX-targetLod-1 || Z > BoundsZ-targetLod-1, targetLod, Cache);
                _stitcher.Process(output.StaticData, borders.StaticData,
                    new Vector3(Lod*BlockSize, 0, Lod*BlockSize), new Vector3(Chunk.Width-Lod*BlockSize, 0, Chunk.Width-Lod*BlockSize),
                    new Vector3(targetLod*BlockSize,0, targetLod*BlockSize), new Vector3(Chunk.Width-targetLod*BlockSize, 0, Chunk.Width-targetLod*BlockSize));
                _waterStitcher.Process(output.WaterData, borders.WaterData,
                    new Vector3(Lod * BlockSize, 0, Lod * BlockSize), new Vector3(Chunk.Width - Lod * BlockSize, 0, Chunk.Width - Lod * BlockSize),
                    new Vector3(targetLod * BlockSize, 0, targetLod * BlockSize), new Vector3(Chunk.Width - targetLod * BlockSize, 0, Chunk.Width - targetLod * BlockSize));
                output.StaticData += borders.StaticData;
                output.WaterData += borders.WaterData;
                output.Failed |= borders.Failed;
                output.HasNoise3D |= borders.HasNoise3D;
                output.HasWater |= borders.HasWater;
            }
            for (var k = 0; k < output.StaticData.Vertices.Count; k++) output.StaticData.Extradata.Add(0);
            output.WaterData = _waterPatcher.Process(output.WaterData, Lod);
            
            output.StaticData.Translate(new Vector3(OffsetX, 0, OffsetZ));
            output.WaterData.Translate(new Vector3(OffsetX, 0, OffsetZ));

            _parent.SetTerrainVertices(output);
            return output;
        }

        private ChunkMeshBuildOutput CreateTerrain(Block[][][] Blocks, Func<int, int, int, bool> Filter, int Lod, RegionCache Cache)
        {
            var failed = false;
            var next = false;
            var hasNoise3D = false;
            var hasWater = false;

            var blockData = new VertexData();
            var waterData = new VertexData();
            var cell = new GridCell
            {
                P = new Vector3[8],
                Type = new BlockType[8],
                Density = new double[8]
            };
                for (var x = 0; x < BoundsX && !failed; x += Lod)
                {
                    next = !next;
                    for (var y = 0; y < BoundsY && !failed; y += 1)
                    {
                        for (var z = 0; z < BoundsZ && !failed; z += Lod)
                        {
                            next = !next;

                            if (Sparsity != null && (y < Sparsity.MiniumHeight || y > Sparsity.MaximumHeight)) continue;
                            if (!Filter(x, y, z)) continue;
                            if (Blocks[x] == null || Blocks[x][y] == null || y == BoundsY - 1 || y == 0) continue;

                            Helper.CreateCell(ref cell, x, y, z, true,
                                Blocks[x][y][z].Type == BlockType.Water && Blocks[x][y + 1][z].Type == BlockType.Air,
                                Lod,
                                out var success);

                            if (!(Blocks[x][y][z].Type == BlockType.Water &&
                                  Blocks[x][y + 1][z].Type == BlockType.Air) &&
                                !MarchingCubes.Usable(0f, cell)) continue;
                            if (!success && y < BoundsY - 2) failed = true;

                            Vector4 color;
                            if (Blocks[x][y][z].Type == BlockType.Water && Blocks[x][y + 1][z].Type == BlockType.Air)
                            {
                                var regionPosition =
                                    new Vector3(cell.P[0].X * BlockSize + OffsetX, 0,
                                        cell.P[0].Z * BlockSize + OffsetZ);

                                RegionColor region = Cache.GetAverageRegionColor(regionPosition);

                                color = region.WaterColor;
                                IsoSurfaceCreator.CreateWaterQuad(BlockSize, cell, next,
                                    new Vector3(BlockSize, 1, BlockSize), Lod, color, waterData);
                                hasWater = true;
                            }

                            if (Blocks[x][y][z].Type == BlockType.Water)
                            {
                                if (Blocks[x][y][z].Type == BlockType.Water &&
                                    Blocks[x][y + 1][z].Type == BlockType.Air)
                                {
                                    Helper.CreateCell(ref cell, x, y, z, true, false, Lod,
                                        out success);
                                }

                                if (!success && y < BoundsY - 2) failed = true;

                                if (!MarchingCubes.Usable(0f, cell)) continue;

                                var regionPosition = new Vector3(cell.P[0].X + OffsetX, 0, cell.P[0].Z + OffsetZ);
                                var region = Cache.GetAverageRegionColor(regionPosition);
                                color = Helper.GetColor(cell, region, Lod);
                                MarchingCubes.Process(0f, cell, color, next, blockData);
                            }
                            else
                            {
                                var regionPosition =
                                    new Vector3(cell.P[0].X + OffsetX, 0, cell.P[0].Z + OffsetZ);

                                RegionColor region = Cache.GetAverageRegionColor(regionPosition);

                                color = Helper.GetColor(cell, region, Lod);

                                MarchingCubes.Process(0f, cell, color, next, blockData);
                            }
                        }
                    }
                }
            return new ChunkMeshBuildOutput(blockData, waterData, failed, hasNoise3D, hasWater);
        }
    }
}
