using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.IO;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Geometry;
using Hedra.Engine.Rendering.Isosurface;
using Hedra.Rendering;
using MeshDecimator;
using MeshDecimator.Algorithms;
using OpenTK;
using UnityMeshSimplifier;
using Vector3d = MeshDecimator.Math.Vector3d;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class ChunkTerrainMeshBuilder
    {
        private const int CollisionMeshLod = 2;
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

        public ChunkMeshBuildOutput CreateTerrainMesh2(Block[][][] Blocks, int Lod, RegionCache Cache)
        {
            var output = CreateTerrain(Blocks, (X,Y,Z) => X != 0 && Z != 0 && X != BoundsX-1 && Z != BoundsZ-1, 1, Lod, Cache, true, true);
            Simplify(output.StaticData, Lod);
            const int targetLod = 1;
            var borders = CreateTerrain(Blocks,
                (X, Y, Z) => X < targetLod || Z < targetLod || X > BoundsX-targetLod-1 || Z > BoundsZ-targetLod-1, targetLod, Lod, Cache, true, true);
            /*
            _stitcher.Process(output.StaticData, borders.StaticData,
                new Vector3(Lod*BlockSize, 0, Lod*BlockSize), new Vector3(Chunk.Width-Lod*BlockSize, 0, Chunk.Width-Lod*BlockSize),
                new Vector3(targetLod*BlockSize,0, targetLod*BlockSize), new Vector3(Chunk.Width-targetLod*BlockSize, 0, Chunk.Width-targetLod*BlockSize));*/
            /*_waterStitcher.Process(output.WaterData, borders.WaterData,
                new Vector3(Lod * BlockSize, 0, Lod * BlockSize), new Vector3(Chunk.Width - Lod * BlockSize, 0, Chunk.Width - Lod * BlockSize),
                new Vector3(targetLod * BlockSize, 0, targetLod * BlockSize), new Vector3(Chunk.Width - targetLod * BlockSize, 0, Chunk.Width - targetLod * BlockSize));*/
            output.StaticData += borders.StaticData;
            output.WaterData += borders.WaterData;
            output.Failed |= borders.Failed;
            output.HasNoise3D |= borders.HasNoise3D;
            output.HasWater |= borders.HasWater;
            for (var k = 0; k < output.StaticData.Vertices.Count; k++) output.StaticData.Extradata.Add(0);

            output.StaticData.Translate(new Vector3(OffsetX, 0, OffsetZ));
            output.WaterData.Translate(new Vector3(OffsetX, 0, OffsetZ));
            return output;
        }
        
        public ChunkMeshBuildOutput CreateTerrainMesh(Block[][][] Blocks, int Lod, RegionCache Cache)
        {
            var output = CreateTerrain(Blocks, (X, Y, Z) => true, 1, Lod, Cache, true, true);
            for (var k = 0; k < output.StaticData.Vertices.Count; k++) output.StaticData.Extradata.Add(0);

            Simplify(output.StaticData, Lod);
            
            output.StaticData.Translate(new Vector3(OffsetX, 0, OffsetZ));
            output.WaterData.Translate(new Vector3(OffsetX, 0, OffsetZ));
            return output;
        }

        private void Simplify(VertexData Data, int Lod)
        {
            var lods = new Dictionary<int, float>
            {
                {1, 0.5f},
                {2, 0.2f},
                {4, 0.1f},
                {8, 0.05f}
            };
            Data.Smooth();
            var detector = new ChunkMeshBorderDetector();
            var border = detector.ProcessEntireBorder(Data.Vertices.ToArray(), Vector3.Zero, new Vector3(Chunk.Width - Lod, 0, Chunk.Width));
            Rendering.MeshOptimizer.MeshOptimizer.Simplify(Data, border.Select(V => (uint)Data.Vertices.IndexOf(V)).ToArray(), lods[Lod]);
            Data.Flat();
        }
        
        /*                                var algorithm = (FastQuadricMeshSimplification) MeshDecimation.CreateAlgorithm(Algorithm.Default);
                algorithm.PreserveFoldovers = true;
                algorithm.PreserveSeams = true;
                algorithm.PreserveBorders = true;
                algorithm.EnableSmartLink = false;
                algorithm.VertexLinkDistanceSqr = 0.0025f;
                var targetTriangleCount = (int)(((int)(data.Indices.Count / 3)) * lods[Lod]);
                var sourceMesh = new Mesh(data.Vertices.Select(V => new Vector3d(V.X, V.Y, V.Z)).ToArray(), data.Indices.Select(I => (int)I).ToArray());
                sourceMesh.Normals = data.Normals.Select(V => new MeshDecimator.Math.Vector3(1, 1, 1)).ToArray();
                sourceMesh.Colors = data.Colors.Select(V => new MeshDecimator.Math.Vector4(V.X, V.Y, V.Z, V.W)).ToArray();
                var mesh = MeshDecimation.DecimateMesh(algorithm, sourceMesh, targetTriangleCount);
                output.StaticData = new VertexData()
                {
                    Indices = mesh.Indices.Select(I => (uint) I).ToList(),
                    Vertices = mesh.Vertices.Select(V => new Vector3((float)V.x, (float)V.y, (float)V.z)).ToList(),
                    Normals = mesh.Normals.Select(V => new Vector3(V.x, V.y, V.z)).ToList(),
                    Colors = mesh.Colors.Select(V => new Vector4(V.x, V.y, V.z, V.w)).ToList()
                };
                output.StaticData.Flat();*/

        private static ChunkMeshBuildOutput Merge(params ChunkMeshBuildOutput[] Outputs)
        {
            var staticData = new VertexData();
            var waterData = new VertexData();
            var instanceData = new VertexData();
            var failed = false;
            var hasNoise = false;
            var hasWater = false;
            for (var i = 0; i < Outputs.Length; ++i)
            {
                if(Outputs[i] == null) continue;
                staticData += Outputs[i].StaticData;
                waterData += Outputs[i].WaterData;
                instanceData += Outputs[i].InstanceData;
                failed |= Outputs[i].Failed;
                hasNoise |= Outputs[i].HasNoise3D;
                hasWater |= Outputs[i].HasWater;
            }
            return new ChunkMeshBuildOutput(staticData, waterData, instanceData, failed, hasNoise, hasWater);
        }

        public VertexData CreateTerrainCollisionMesh(Block[][][] Blocks, RegionCache Cache)
        {
            return CreateTerrain(Blocks, (X,Y,Z) => true, CollisionMeshLod, 1, Cache, false, false).StaticData;
        }

        private ChunkMeshBuildOutput CreateTerrainTransvoxel(Block[][][] Blocks, Func<int, int, int, bool> Filter, int Lod, int ColorLod, RegionCache Cache, bool ProcessWater, bool ProcessColors)
        {
            var verts = new List<Vertex>();
            var indices = new List<int>();
            var cache = new RegularCellCache();
            var volume = new VolumeData(Blocks);
            for (var x = 2; x < BoundsX-2; x += 1)
            {
                for (var y = 2; y < BoundsY-2; y += 1)
                {
                    for (var z = 2; z < BoundsZ-2; z += 1)
                    {
                        Transvoxel.PolygonizeRegularCell(Vector3i.Zero, new Vector3(x, y, z), new Vector3i(x, y, z), volume, 0, 4, ref verts, ref indices, ref cache);
                    }
                }
            }

            return new ChunkMeshBuildOutput(new VertexData
            {
                Vertices = verts.Select(V => V.Primary).ToList(),
                Indices = indices.Select(I => (uint)I).ToList(),
                Normals = verts.Select(V => V.Normal).ToList(),
                Colors = Enumerable.Repeat(new Vector4(0, .75f, 0, 1), verts.Count).ToList()
            }, VertexData.Empty, new VertexData(), false, true, false);
        }
        
        private ChunkMeshBuildOutput CreateTerrain(Block[][][] Blocks, Func<int, int, int, bool> Filter, int Lod, int ColorLod, RegionCache Cache, bool ProcessWater, bool ProcessColors)
        {
            var failed = false;
            var next = false;
            var hasNoise3D = false;
            var hasWater = false;
            var blockData = new VertexData();
            var waterData = new VertexData();
            var vertexBuffer = MarchingCubes.NewVertexBuffer();
            var triangleBuffer = MarchingCubes.NewTriangleBuffer();
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

                        if (y < Sparsity.MinimumHeight || y > Sparsity.MaximumHeight) continue;
                        if (!Filter(x, y, z)) continue;
                        if (Blocks[x] == null || Blocks[x][y] == null || y == BoundsY - 1 || y == 0) continue;

                        var isWaterCell = Blocks[x][y][z].Type == BlockType.Water &&
                                          Blocks[x][y + 1][z].Type == BlockType.Air;
                        Helper.CreateCell(ref cell, ref x, ref y, ref z, ref isWaterCell, ref Lod, out var success);

                        if (!(Blocks[x][y][z].Type == BlockType.Water &&
                              Blocks[x][y + 1][z].Type == BlockType.Air) &&
                            !MarchingCubes.Usable(0f, cell)) continue;
                        if (!success && y < BoundsY - 2) failed = true;

                        if (Blocks[x][y][z].Type == BlockType.Water && Blocks[x][y + 1][z].Type == BlockType.Air &&
                            ProcessWater)
                        {
                            var regionPosition =
                                new Vector3(cell.P[0].X * BlockSize + OffsetX, 0,
                                    cell.P[0].Z * BlockSize + OffsetZ);

                            var region = Cache.GetAverageRegionColor(regionPosition);

                            IsoSurfaceCreator.CreateWaterQuad(BlockSize, cell, next,
                                new Vector3(BlockSize, 1, BlockSize), Lod, region.WaterColor, waterData);
                            hasWater = true;
                        }

                        if (Blocks[x][y][z].Type == BlockType.Water)
                        {
                            if (Blocks[x][y][z].Type == BlockType.Water &&
                                Blocks[x][y + 1][z].Type == BlockType.Air)
                            {
                                var waterCell = false;
                                Helper.CreateCell(ref cell, ref x, ref y, ref z, ref waterCell, ref Lod, out success);
                            }

                            if (!success && y < BoundsY - 2) failed = true;

                            if (!MarchingCubes.Usable(0f, cell)) continue;

                            PolygoniseCell(ref cell, ref ProcessColors, ref next, ref blockData, ref ColorLod, ref vertexBuffer, ref triangleBuffer, ref Cache);
                        }
                        else
                        {
                            PolygoniseCell(ref cell, ref ProcessColors, ref next, ref blockData, ref ColorLod, ref vertexBuffer, ref triangleBuffer, ref Cache);
                        }
                    }
                }
            }

            return new ChunkMeshBuildOutput(blockData, waterData, new VertexData(), failed, hasNoise3D, hasWater);
        }

        private void PolygoniseCell(ref GridCell Cell, ref bool ProcessColors, ref bool Next, ref VertexData BlockData,
            ref int ColorLod, ref Vector3[] VertexBuffer, ref Triangle[] TriangleBuffer, ref RegionCache Cache)
        {
            MarchingCubes.Polygonise(ref Cell, 0, ref VertexBuffer, ref TriangleBuffer, out var triangleCount);
            var color = Vector4.Zero;
            if (ProcessColors)
            {
                var normal = CalculateAverageNormal(ref TriangleBuffer, ref triangleCount);
                var regionPosition = new Vector3(Cell.P[0].X + OffsetX, 0, Cell.P[0].Z + OffsetZ);
                var region = Cache.GetAverageRegionColor(regionPosition);
                color = Helper.GetColor(ref Cell, region, ColorLod, ref normal);
            }
            MarchingCubes.Build(ref BlockData, ref color, ref TriangleBuffer, ref triangleCount, ref Next);
        }

        private static Vector3 CalculateAverageNormal(ref Triangle[] TriangleBuffer, ref int TriangleCount)
        {
            if(TriangleCount == 0) return Vector3.One;
            var averageNormal = Vector3.Zero;
            for (var i = 0; i < TriangleCount; ++i)
                averageNormal += Vector3.Cross(TriangleBuffer[i].Vertices[1] - TriangleBuffer[i].Vertices[0], TriangleBuffer[i].Vertices[2] - TriangleBuffer[i].Vertices[0]).NormalizedFast();
            return averageNormal / TriangleCount;
        }
    }
}
