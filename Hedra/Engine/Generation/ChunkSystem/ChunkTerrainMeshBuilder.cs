using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Native;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Geometry;
using Hedra.Rendering;
using Microsoft.Scripting.Utils;
using OpenTK;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class ChunkTerrainMeshBuilder
    {
        private const int CollisionMeshLod = 2;
        private readonly Chunk _parent;
        private readonly Dictionary<int, float> _lodMap = new Dictionary<int, float>
        {
            {1, 0.5f},
            {2, 0.2f},
            {4, 0.125f},
            {8, 0.075f}
        };
        public ChunkSparsity Sparsity { get; set; }

        public ChunkTerrainMeshBuilder(Chunk Parent)
        {
            _parent = Parent;
        }

        private int OffsetX => _parent.OffsetX;
        private int OffsetZ => _parent.OffsetZ;
        private static int BoundsX => Chunk.BoundsX;
        private static int BoundsY => Chunk.BoundsY;
        private static int BoundsZ => Chunk.BoundsZ;
        private static float BlockSize => Chunk.BlockSize;

        public ChunkMeshBuildOutput CreateTerrainMesh(int Lod, RegionCache Cache)
        {
            var output = CreateTerrain(Lod, Cache, true, true);
            for (var k = 0; k < output.StaticData.Vertices.Count; k++) output.StaticData.Extradata.Add(0);

            Simplify(output.StaticData, Lod);

            output.StaticData.Translate(new Vector3(OffsetX, 0, OffsetZ));
            output.WaterData.Translate(new Vector3(OffsetX, 0, OffsetZ));
            return output;
        }

        private void Simplify(VertexData Data, int Lod)
        {
            Data.UniqueVertices();
            var detector = new ChunkMeshBorderDetector();
            var border = detector.ProcessEntireBorder(Data, Vector3.Zero, new Vector3(Chunk.Width, 0, Chunk.Width));
            MeshOptimizer.Simplify(Data, border, _lodMap[Lod]);
            Data.Flat();
        }

        public VertexData CreateTerrainCollisionMesh(RegionCache Cache)
        {
            return CreateTerrain(1, Cache, false, false, CollisionMeshLod, CollisionMeshLod).StaticData;
        }
        
        private ChunkMeshBuildOutput CreateTerrain(int Lod, RegionCache Cache, bool ProcessWater, bool ProcessColors, int HorizontalIncrement = 1, int VerticalIncrement = 1)
        {
            var failed = false;
            var blockData = new VertexData();
            var waterData = new VertexData();
            var helper = new ChunkTerrainMeshBuilderHelper(_parent, Lod);

            IterateAndBuild(helper, ref failed, ProcessWater, ProcessColors, Cache, blockData, waterData, HorizontalIncrement, VerticalIncrement);

            return new ChunkMeshBuildOutput(blockData, waterData, new VertexData(), failed);
        }

        private void IterateAndBuild(ChunkTerrainMeshBuilderHelper Helper, ref bool failed, bool ProcessWater, bool ProcessColors, RegionCache Cache, VertexData blockData, VertexData waterData, int HorizontalIncrement, int VerticalIncrement)
        {

            Loop(Helper, HorizontalIncrement, VerticalIncrement, ProcessColors, false, ref blockData, ref failed, ref Cache);
            if (ProcessWater && _parent.HasWater)
            {
                Loop(Helper, 1, 1, ProcessColors, true, ref waterData, ref failed, ref Cache);
                waterData.UniqueVertices();
                var detector = new ChunkMeshBorderDetector();
                var set = new HashSet<uint>(detector.ProcessEntireBorder(waterData, Vector3.Zero, new Vector3(Chunk.Width, 0, Chunk.Width)));
                //for(var i = 0; i < 2; ++i)
                //    MeshAnalyzer.ApplySmoothing(waterData, set);
                waterData.Flat();
            }
        }

        private void Loop(ChunkTerrainMeshBuilderHelper Helper, int HorizontalSkip, int VerticalSkip, bool ProcessColors, bool isWater, ref VertexData blockData, ref bool failed, ref RegionCache Cache)
        {
            var vertexBuffer = MarchingCubes.NewVertexBuffer();
            var triangleBuffer = MarchingCubes.NewTriangleBuffer();
            var cell = new GridCell
            {
                P = new Vector3[8],
                Type = new BlockType[8],
                Density = new double[8]
            };
            var next = false;
            for (var x = 0; x < BoundsX && !failed; x+=HorizontalSkip)
            {
                next = !next;
                for (var y = 0; y < BoundsY && !failed; y+=VerticalSkip)
                {
                    for (var z = 0; z < BoundsZ && !failed; z+=HorizontalSkip)
                    {
                        next = !next;

                        if (y < Sparsity.MinimumHeight || y > Sparsity.MaximumHeight) continue;
                        if (y == BoundsY - 1 || y == 0) continue;
                        
                        Helper.CreateCell(ref cell, ref x, ref y, ref z, isWater, HorizontalSkip, VerticalSkip, out var success);
                        if (!MarchingCubes.Usable(0f, cell)) continue;
                        if (!success && y < BoundsY - 2) failed = true;

                        var color = Vector4.Zero;
                        if (isWater)
                        {
                            var regionPosition = new Vector3(cell.P[0].X + OffsetX, 0, cell.P[0].Z + OffsetZ);
                            var region = Cache.GetAverageRegionColor(regionPosition);
                            color = new Vector4(region.WaterColor.Xyz, 1);
                        }
                        else
                        {
                            color = GetCellColor(Helper, ref cell, ref ProcessColors, ref Cache, false);
                        }

                        PolygoniseCell(ref cell, ref next, ref blockData, ref vertexBuffer, ref triangleBuffer, color, isWater);
                    }
                }
            }
        }

        private static void PolygoniseCell(ref GridCell Cell, ref bool Next, ref VertexData BlockData, ref Vector3[] VertexBuffer, ref Triangle[] TriangleBuffer, Vector4 Color, bool IsWater)
        {
            MarchingCubes.Polygonise(ref Cell, 0, ref VertexBuffer, ref TriangleBuffer, out var triangleCount);
            MarchingCubes.Build(ref BlockData, ref Color, ref TriangleBuffer, ref triangleCount, ref Next, ref IsWater);
        }

        private Vector4 GetCellColor(ChunkTerrainMeshBuilderHelper Helper, ref GridCell Cell, ref bool ProcessColors, ref RegionCache Cache, bool isWaterCell)
        {
            var color = Vector4.Zero;
            if (ProcessColors)
            {
                var regionPosition = new Vector3(Cell.P[0].X + OffsetX, 0, Cell.P[0].Z + OffsetZ);
                var region = Cache.GetAverageRegionColor(regionPosition);
                color = !isWaterCell 
                    ? Helper.GetColor(ref Cell, region) 
                    : Cache.GetAverageRegionColor(Cell.P[0]).WaterColor;
            }

            return color;
        }
    }
}
