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
        public ChunkTerrainMeshBuilderHelper Helper { get; }

        public ChunkTerrainMeshBuilder(Chunk Parent)
        {
            Helper = new ChunkTerrainMeshBuilderHelper(Parent);
            _parent = Parent;
        }

        private int OffsetX => _parent.OffsetX;
        private int OffsetZ => _parent.OffsetZ;
        private int BoundsX => _parent.BoundsX;
        private int BoundsY => _parent.BoundsY;
        private int BoundsZ => _parent.BoundsZ;
        private static float BlockSize => Chunk.BlockSize;

        public ChunkMeshBuildOutput CreateTerrainMesh(Block[][][] Blocks, int Lod, RegionCache Cache)
        {
            var output = CreateTerrain(Blocks, Lod, Cache, true, true);
            for (var k = 0; k < output.StaticData.Vertices.Count; k++) output.StaticData.Extradata.Add(0);

            Simplify(output.StaticData, Lod);

            output.StaticData.Translate(new Vector3(OffsetX, 0, OffsetZ));
            output.WaterData.Translate(new Vector3(OffsetX, 0, OffsetZ));
            return output;
        }

        private void Simplify(VertexData Data, int Lod)
        {
            Data.Smooth();
            var detector = new ChunkMeshBorderDetector();
            var border = detector.ProcessEntireBorder(Data, Vector3.Zero, new Vector3(Chunk.Width, 0, Chunk.Width));
            MeshOptimizer.Simplify(Data, border, _lodMap[Lod]);
            Data.Flat();
        }

        private static ChunkMeshBuildOutput Merge(params ChunkMeshBuildOutput[] Outputs)
        {
            var staticData = new VertexData();
            var waterData = new VertexData();
            var instanceData = new VertexData();
            var failed = false;
            var hasWater = false;
            for (var i = 0; i < Outputs.Length; ++i)
            {
                if(Outputs[i] == null) continue;
                staticData += Outputs[i].StaticData;
                waterData += Outputs[i].WaterData;
                instanceData += Outputs[i].InstanceData;
                failed |= Outputs[i].Failed;
                hasWater |= Outputs[i].HasWater;
            }
            return new ChunkMeshBuildOutput(staticData, waterData, instanceData, failed, hasWater);
        }

        public VertexData CreateTerrainCollisionMesh(Block[][][] Blocks, RegionCache Cache)
        {
            return CreateTerrain(Blocks, CollisionMeshLod, Cache, false, false).StaticData;
        }
        
        private ChunkMeshBuildOutput CreateTerrain(Block[][][] Blocks, int Lod, RegionCache Cache, bool ProcessWater, bool ProcessColors)
        {
            var failed = false;
            var hasWater = false;
            var blockData = new VertexData();
            var waterData = new VertexData();
            var densityGrid = Helper.BuildDensityGrid(Lod);

            IterateAndBuild(densityGrid, Blocks, ref failed, ref hasWater, ProcessWater, ProcessColors, Cache, blockData, waterData);

            return new ChunkMeshBuildOutput(blockData, waterData, new VertexData(), failed, hasWater);
        }

        private void IterateAndBuild(SampledBlock[][][] densityGrid, Block[][][] Blocks, ref bool failed,
            ref bool hasWater, bool ProcessWater, bool ProcessColors, RegionCache Cache, VertexData blockData, VertexData waterData)
        {

            Loop(densityGrid, Blocks, 1, ProcessColors, false, ref blockData, ref failed, ref Cache);
            if(ProcessWater)
                Loop(densityGrid, Blocks, 2, ProcessColors, true, ref blockData, ref failed, ref Cache);
            
            hasWater = waterData.Vertices.Count > 0;
        }

        private void Loop(SampledBlock[][][] densityGrid, Block[][][] Blocks, int Lod, bool ProcessColors, bool isWater, ref VertexData blockData, ref bool failed, ref RegionCache Cache)
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
            for (var x = 0; x < BoundsX && !failed; x+=Lod)
            {
                next = !next;
                for (var y = 0; y < BoundsY && !failed; y+=Lod)
                {
                    for (var z = 0; z < BoundsZ && !failed; z+=Lod)
                    {
                        next = !next;

                        if (y < Sparsity.MinimumHeight || y > Sparsity.MaximumHeight) continue;
                        if (Blocks[x] == null || Blocks[x][y] == null || y == BoundsY - 1 || y == 0) continue;
                        
                        Helper.CreateCell(densityGrid, ref cell, ref x, ref y, ref z, false, out var success);
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
                            color = GetCellColor(densityGrid, ref cell, ref ProcessColors, ref Cache, false);
                        }

                        PolygoniseCell(ref cell, ref next, ref blockData, ref vertexBuffer, ref triangleBuffer, color, false);
                    }
                }
            }
        }

        private static void PolygoniseCell(ref GridCell Cell, ref bool Next, ref VertexData BlockData, ref Vector3[] VertexBuffer, ref Triangle[] TriangleBuffer, Vector4 Color, bool IsWater)
        {
            MarchingCubes.Polygonise(ref Cell, 0, ref VertexBuffer, ref TriangleBuffer, out var triangleCount);
            MarchingCubes.Build(ref BlockData, ref Color, ref TriangleBuffer, ref triangleCount, ref Next);
        }

        private Vector4 GetCellColor(SampledBlock[][][] Grid, ref GridCell Cell, ref bool ProcessColors, ref RegionCache Cache, bool isWaterCell)
        {
            var color = Vector4.Zero;
            if (ProcessColors)
            {
                var regionPosition = new Vector3(Cell.P[0].X + OffsetX, 0, Cell.P[0].Z + OffsetZ);
                var region = Cache.GetAverageRegionColor(regionPosition);
                color = !isWaterCell 
                    ? Helper.GetColor(Grid, ref Cell, region) 
                    : Cache.GetAverageRegionColor(Cell.P[0]).WaterColor;
            }

            return color;
        }
    }
}
