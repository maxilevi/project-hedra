using System;
using System.Collections.Generic;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class ChunkTerrainMeshBuilder
    {
        private readonly Chunk _parent;
        public ChunkTerrainMeshBuilderHelper Helper { get; }

        public ChunkTerrainMeshBuilder(Chunk Parent)
        {
            _parent = Parent;
            Helper = new ChunkTerrainMeshBuilderHelper(Parent);
        }

        private int Lod => _parent.Lod;
        private int OffsetX => _parent.OffsetX;
        private int OffsetZ => _parent.OffsetZ;
        private int BoundsX => _parent.BoundsX;
        private int BoundsY => _parent.BoundsY;
        private int BoundsZ => _parent.BoundsZ;
        private static float BlockSize => Chunk.BlockSize;
        private Block[][][] Voxels => _parent.Voxels;

        public ChunkMeshBuildOutput CreateTerrainMesh(Chunk[] Neighbours)
        {
            try
            {
                var failed = false;
                var next = false;
                var hasNoise3D = false;
                var hasWater = false;

                lock (Voxels)
                {

                    Chunk rightChunk = Neighbours?[0];
                    Chunk frontChunk = Neighbours?[1];
                    Chunk rightFrontChunk = Neighbours?[2];
                    Chunk leftFrontChunk = Neighbours?[3];
                    Chunk rightBackChunk = Neighbours?[4];
                    Chunk leftBackChunk = Neighbours?[5];
                    Chunk leftChunk = Neighbours?[6];
                    Chunk backChunk = Neighbours?[7];

                    var addonColors = new List<Vector4>();
                    var blockData = new VertexData();
                    var waterData = new VertexData();
                    var cell = new GridCell
                    {
                        P = new Vector3[8],
                        Type = new BlockType[8],
                        Density = new double[8]
                    };

                    for (var y = 0; y < BoundsY; y += 1)
                    for (var x = 0; x < BoundsX; x += Lod)
                    {
                        next = !next;
                        for (var z = 0; z < BoundsZ; z += Lod)
                        {
                            next = !next;

                            if (Voxels[x] == null || Voxels[x][y] == null || y == BoundsY - 1 || y == 0) continue;
                            var success = true;

                            Helper.CreateCell(ref cell, x, y, z, rightChunk, frontChunk, rightFrontChunk, leftBackChunk,
                                rightBackChunk, leftFrontChunk, backChunk, leftChunk,
                                BoundsX, BoundsY, BoundsZ, true,
                                Voxels[x][y][z].Type == BlockType.Water && Voxels[x][y + 1][z].Type == BlockType.Air,
                                out success);

                            if (!(Voxels[x][y][z].Type == BlockType.Water && Voxels[x][y + 1][z].Type == BlockType.Air) && !MarchingCubes.Usable(0f, cell)) continue;
                            if (Voxels[x][y][z].Noise3D) hasNoise3D = true;
                            if (!success && y < BoundsY - 2) failed = true;

                            Vector4 color;
                            if (Voxels[x][y][z].Type == BlockType.Water && Voxels[x][y + 1][z].Type == BlockType.Air)
                            {
                                var regionPosition =
                                    new Vector3(cell.P[0].X * BlockSize + OffsetX, 0,
                                        cell.P[0].Z * BlockSize + OffsetZ);

                                RegionColor region = World.BiomePool.GetAverageRegionColor(regionPosition);

                                color = region.WaterColor;
                                IsoSurfaceCreator.CreateWaterQuad(BlockSize, cell, next,
                                    new Vector3(BlockSize, 1, BlockSize), Lod, color, waterData);
                                hasWater = true;
                            }

                            if (Voxels[x][y][z].Type == BlockType.Water)
                            {
                                if (Voxels[x][y][z].Type == BlockType.Water &&
                                    Voxels[x][y + 1][z].Type == BlockType.Air)
                                    Helper.CreateCell(ref cell, x, y, z, rightChunk, frontChunk, rightFrontChunk,
                                        leftBackChunk,
                                        rightBackChunk, leftFrontChunk, backChunk, leftChunk,
                                        BoundsX, BoundsY, BoundsZ, true, false, out success);

                                if (!MarchingCubes.Usable(0f, cell)) continue;

                                var regionPosition = new Vector3(cell.P[0].X + OffsetX, 0, cell.P[0].Z + OffsetZ);
                                RegionColor region = World.BiomePool.GetAverageRegionColor(regionPosition);
                                color = Helper.GetColor(cell, Voxels[x][y][z].Type, rightChunk, frontChunk,
                                    rightFrontChunk,
                                    leftBackChunk, rightBackChunk, leftFrontChunk, backChunk,
                                    leftChunk, BoundsX, BoundsY, BoundsZ, addonColors, region);
                                MarchingCubes.Process(0f, cell, color, next, blockData);
                            }
                            else
                            {
                                var regionPosition =
                                    new Vector3(cell.P[0].X + OffsetX, 0, cell.P[0].Z + OffsetZ);

                                RegionColor region = World.BiomePool.GetAverageRegionColor(regionPosition);

                                color = Helper.GetColor(cell, Voxels[x][y][z].Type, rightChunk, frontChunk,
                                    rightFrontChunk,
                                    leftBackChunk, rightBackChunk, leftFrontChunk, backChunk,
                                    leftChunk, BoundsX, BoundsY, BoundsZ, addonColors, region);

                                MarchingCubes.Process(0f, cell, color, next, blockData);
                            }
                        }
                    }
                    for (var k = 0; k < blockData.Vertices.Count; k++) blockData.ExtraData.Add(0);
                    blockData = MeshSimplifier.Simplify(blockData, Lod);
                    waterData = MeshSimplifier.Simplify(waterData, Lod);
                    blockData.Transform(new Vector3(OffsetX, 0, OffsetZ));
                    waterData.Transform(new Vector3(OffsetX, 0, OffsetZ));

                    var output = new ChunkMeshBuildOutput(blockData, waterData, failed, hasNoise3D, hasWater);
                    _parent.SetTerrainVertices(output);
                    return output;
                }
            }
            catch (Exception e)
            {
                Log.WriteLine(
                    "---- Probably a sync error while building the chunk mesh ---- " + Environment.NewLine + e);
            }
            return null;
        }

    }
}
