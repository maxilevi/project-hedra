using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Native;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Geometry;
using Hedra.Framework;
using Hedra.Numerics;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class ChunkTerrainMeshBuilder
    {
        private const int CollisionMeshLod = 2;
        private static readonly object WaterLock;
        private static readonly Dictionary<Vector3, HashSet<Vector3>> ChunkWaterMap;
        private static readonly Dictionary<Vector3, Vector3> WaterMappings;

        private static readonly Dictionary<int, float> LODMap = new Dictionary<int, float>
        {
            { 1, 0.5f },
            { 2, 0.2f },
            { 4, 0.125f },
            { 8, 0.075f }
        };

        private static readonly Dictionary<int, float> WaterLODMap = new Dictionary<int, float>
        {
            { 1, 0.4f },
            { 2, 0.2f },
            { 4, 0.125f },
            { 8, 0.075f }
        };

        private readonly Chunk _parent;

        static ChunkTerrainMeshBuilder()
        {
            WaterLock = new object();
            ChunkWaterMap = new Dictionary<Vector3, HashSet<Vector3>>();
            WaterMappings = new Dictionary<Vector3, Vector3>();
        }

        public ChunkTerrainMeshBuilder(Chunk Parent)
        {
            _parent = Parent;
        }

        public ChunkSparsity Sparsity { get; set; }

        private int OffsetX => _parent.OffsetX;
        private int OffsetZ => _parent.OffsetZ;
        private static int BoundsX => Chunk.BoundsX;
        private static int BoundsY => Chunk.BoundsY;
        private static int BoundsZ => Chunk.BoundsZ;
        private static float BlockSize => Chunk.BlockSize;

        public static int WaterMappingsCount
        {
            get
            {
                lock (WaterLock)
                {
                    return WaterMappings.Count;
                }
            }
        }

        public static int ChunkWaterMapCount
        {
            get
            {
                lock (WaterLock)
                {
                    return ChunkWaterMap.Count;
                }
            }
        }

        public ChunkMeshBuildOutput CreateTerrainMesh(IAllocator Allocator, int Lod, RegionCache Cache)
        {
            var output = CreateTerrain(Allocator, Lod, Cache, true, true);
            for (var k = 0; k < output.StaticData.Vertices.Count; k++) output.StaticData.Extradata.Add(0);

            Simplify(Allocator, output.StaticData, Lod);

            output.StaticData.Translate(new Vector3(OffsetX, 0, OffsetZ));
            output.WaterData.Translate(new Vector3(OffsetX, 0, OffsetZ));
            return output;
        }

        private static void Simplify(IAllocator Allocator, NativeVertexData Data, int Lod)
        {
            Data.UniqueVertices();
            var detector = new ChunkMeshBorderDetector();
            var border = detector.ProcessEntireBorder(Data, Vector3.Zero, new Vector3(Chunk.Width, 0, Chunk.Width));
            MeshOptimizer.Simplify(Allocator, Data, border, LODMap[Lod]);
            Data.Flat(Allocator);
        }

        public NativeVertexData CreateTerrainCollisionMesh(RegionCache Cache, IAllocator Allocator)
        {
            var output = CreateTerrain(Allocator, 1, Cache, false, false, CollisionMeshLod);
            var staticData = output.StaticData;
            output.StaticData = null;
            output.Dispose();
            return staticData;
        }

        private void PostProcessWater(IAllocator Allocator, NativeVertexData waterData, int Lod)
        {
            if (waterData.IsEmpty) return;
            waterData.UniqueVertices();
            var detector = new ChunkMeshBorderDetector();
            var border =
                detector.ProcessEntireBorder(waterData, Vector3.Zero, new Vector3(Chunk.Width, 0, Chunk.Width));
            var originalBorder = BuildOriginalBorder(Allocator, waterData, border);
            MeshAnalyzer.ApplySmoothing(waterData.Indices, waterData.Vertices, waterData.Colors, waterData.Normals,
                null, 6);
            //MeshOptimizer.Simplify(Allocator, waterData, border, WaterLODMap[Lod]);
            if (border.Length > 0)
                SnapOrAddVertices(waterData, originalBorder, border, new Vector3(OffsetX, 0, OffsetZ));
            waterData.Flat(Allocator);
        }

        private static NativeArray<Vector3> BuildOriginalBorder(IAllocator Allocator, NativeVertexData Water,
            uint[] Border)
        {
            if (Border.Length == 0) return default;
            var array = new NativeArray<Vector3>(Allocator, Border.Length);
            for (var i = 0; i < Border.Length; ++i) array[i] = Water.Vertices[(int)Border[i]];
            return array;
        }

        private static void SnapOrAddVertices(NativeVertexData Water, NativeArray<Vector3> BorderVertices,
            uint[] Border, Vector3 Offset)
        {
            lock (WaterLock)
            {
                for (var i = 0; i < BorderVertices.Length; ++i)
                {
                    var vertex = BorderVertices[i] + Offset;
                    if (WaterMappings.ContainsKey(vertex))
                    {
                        Water.Vertices[(int)Border[i]] = WaterMappings[vertex] - Offset;
                    }
                    else
                    {
                        if (!ChunkWaterMap.ContainsKey(Offset))
                            ChunkWaterMap[Offset] = new HashSet<Vector3>();
                        ChunkWaterMap[Offset].Add(vertex);
                        WaterMappings.Add(vertex, Water.Vertices[(int)Border[i]] + Offset);
                    }
                }
            }
        }

        private unsafe ChunkMeshBuildOutput CreateTerrain(IAllocator Allocator, int Lod, RegionCache Cache,
            bool ProcessWater, bool ProcessColors, int HorizontalIncrement = 1, int VerticalIncrement = 1)
        {
            var failed = false;
            var blockData = new NativeVertexData(Allocator);
            var waterData = new NativeVertexData(Allocator);
            var grid = stackalloc SampledBlock[ChunkTerrainMeshBuilderHelper.CalculateGridSize(Lod)];
            var helper = new ChunkTerrainMeshBuilderHelper(_parent, Lod, grid);

            IterateAndBuild(helper, ref failed, ProcessWater, ProcessColors, Cache, blockData, waterData,
                HorizontalIncrement, VerticalIncrement);
            PostProcessWater(Allocator, waterData, Lod);

            return new ChunkMeshBuildOutput(blockData, waterData, new NativeVertexData(Allocator), failed);
        }

        private void IterateAndBuild(ChunkTerrainMeshBuilderHelper Helper, ref bool failed, bool ProcessWater,
            bool ProcessColors, RegionCache Cache, NativeVertexData blockData, NativeVertexData waterData,
            int HorizontalIncrement, int VerticalIncrement)
        {
            Loop(Helper, HorizontalIncrement, VerticalIncrement, ProcessColors, false, ref blockData, ref failed,
                ref Cache);
            if (ProcessWater && _parent.HasWater)
                Loop(Helper, 1, 1, ProcessColors, true, ref waterData, ref failed, ref Cache);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void Loop(ChunkTerrainMeshBuilderHelper Helper, int HorizontalSkip, int VerticalSkip,
            bool ProcessColors, bool isWater, ref NativeVertexData blockData, ref bool failed, ref RegionCache Cache)
        {
            var vertexBuffer = MarchingCubes.NewVertexBuffer();
            var triangleBuffer = MarchingCubes.NewTriangleBuffer();
            var isRiverConstant = _parent.IsRiverConstant;
            var cell = new GridCell
            {
                P = new Vector3[8],
                Type = new BlockType[8],
                Density = new double[8]
            };
            var width = BoundsX;
            var height = BoundsY;
            var depth = BoundsZ;
            for (var x = 0; x < width && !failed; x += HorizontalSkip)
            for (var y = 0; y < height && !failed; y += VerticalSkip)
            for (var z = 0; z < depth && !failed; z += HorizontalSkip)
            {
                if (y < Sparsity.MinimumHeight || y > Sparsity.MaximumHeight) continue;
                if (y == BoundsY - VerticalSkip || y == 0) continue;

                Helper.CreateCell(ref cell, x, y, z, isWater, HorizontalSkip, VerticalSkip, out var success);
                if (!MarchingCubes.Usable(0f, cell)) continue;
                if (!success && y < BoundsY - 2) failed = true;

                var color = Vector4.Zero;
                if (isWater)
                {
                    var regionPosition = new Vector3(cell.P[0].X + OffsetX, 0, cell.P[0].Z + OffsetZ);
                    var region = Cache.GetAverageRegionColor(regionPosition);
                    color = new Vector4(region.WaterColor.Xyz(), 1);
                }
                else
                {
                    color = GetCellColor(Helper, ref cell, ref ProcessColors, ref Cache, false);
                }

                PolygoniseCell(ref cell, ref blockData, ref vertexBuffer, ref triangleBuffer, color, ref isWater,
                    ref isRiverConstant);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static void PolygoniseCell(ref GridCell Cell, ref NativeVertexData BlockData,
            ref Vector3[] VertexBuffer, ref Triangle[] TriangleBuffer, Vector4 Color, ref bool IsWater,
            ref bool IsRiverConstant)
        {
            MarchingCubes.Polygonise(ref Cell, 0, ref VertexBuffer, ref TriangleBuffer, out var triangleCount);
            MarchingCubes.Build(ref BlockData, ref Color, ref TriangleBuffer, ref triangleCount, ref IsWater,
                ref IsRiverConstant);
        }

        private Vector4 GetCellColor(ChunkTerrainMeshBuilderHelper Helper, ref GridCell Cell, ref bool ProcessColors,
            ref RegionCache Cache, bool isWaterCell)
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

        public static void ClearMapping(Vector3 Offset)
        {
            lock (WaterLock)
            {
                if (!ChunkWaterMap.ContainsKey(Offset)) return;
                var mappings = ChunkWaterMap[Offset];
                foreach (var point in mappings) WaterMappings.Remove(point);
                ChunkWaterMap.Remove(Offset);
            }
        }
    }
}