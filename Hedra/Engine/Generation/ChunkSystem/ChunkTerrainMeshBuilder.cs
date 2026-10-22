using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
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
            { 1, 0.4f },
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
        private readonly bool _useOptimizedTraversal;

        static ChunkTerrainMeshBuilder()
        {
            WaterLock = new object();
            ChunkWaterMap = new Dictionary<Vector3, HashSet<Vector3>>();
            WaterMappings = new Dictionary<Vector3, Vector3>();
        }

        public ChunkTerrainMeshBuilder(Chunk Parent) : this(Parent, true)
        {
        }

        private ChunkTerrainMeshBuilder(Chunk Parent, bool UseOptimizedTraversal)
        {
            _parent = Parent;
            _useOptimizedTraversal = UseOptimizedTraversal;
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
            var output = CreateTerrain(Allocator, Lod, Cache, true, true, BuildIndexed: _useOptimizedTraversal);
            for (var k = 0; k < output.StaticData.Vertices.Count; k++) output.StaticData.Extradata.Add(0);

            Simplify(Allocator, output.StaticData, Lod, _useOptimizedTraversal);

            output.StaticData.Translate(new Vector3(OffsetX, 0, OffsetZ));
            output.WaterData.Translate(new Vector3(OffsetX, 0, OffsetZ));
            return output;
        }

        private static void Simplify(IAllocator Allocator, NativeVertexData Data, int Lod, bool AlreadyIndexed)
        {
            using var profile = GenerationMetrics.Measure("terrain_simplify");
            if (!AlreadyIndexed)
                using (GenerationMetrics.Measure("terrain_weld")) Data.UniqueVertices();
            var detector = new ChunkMeshBorderDetector();
            uint[] border;
            using (GenerationMetrics.Measure("terrain_border"))
                border = detector.ProcessEntireBorder(Data, Vector3.Zero, new Vector3(Chunk.Width, 0, Chunk.Width));
            using (GenerationMetrics.Measure("terrain_decimate")) MeshOptimizer.Simplify(Allocator, Data, border, LODMap[Lod]);
            using (GenerationMetrics.Measure("terrain_flatten")) Data.Flat(Allocator);
        }

        public NativeVertexData CreateTerrainCollisionMesh(RegionCache Cache, IAllocator Allocator)
        {
            var output = CreateTerrain(Allocator, 1, Cache, false, false, CollisionMeshLod);
            var staticData = output.StaticData;
            output.StaticData = null;
            output.Dispose();
            return staticData;
        }

        private void PostProcessWater(IAllocator Allocator, NativeVertexData waterData, int Lod, bool AlreadyIndexed)
        {
            if (waterData.IsEmpty) return;
            if (!AlreadyIndexed) waterData.UniqueVertices();
            var detector = new ChunkMeshBorderDetector();
            var border =
                detector.ProcessEntireBorder(waterData, Vector3.Zero, new Vector3(Chunk.Width, 0, Chunk.Width));
            var originalBorder = BuildOriginalBorder(Allocator, waterData, border);
            MeshAnalyzer.ApplySmoothing(waterData, null, 6, SmoothNormals: false);
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

        [SkipLocalsInit]
        private unsafe ChunkMeshBuildOutput CreateTerrain(IAllocator Allocator, int Lod, RegionCache Cache,
            bool ProcessWater, bool ProcessColors, int HorizontalIncrement = 1, int VerticalIncrement = 1, bool BuildIndexed = false)
        {
            using var profile = GenerationMetrics.Measure("marching_cubes");
            var failed = false;
            var blockData = new NativeVertexData(Allocator);
            var waterData = new NativeVertexData(Allocator);
            var grid = stackalloc SampledBlock[Lod == 1 ? 0 : ChunkTerrainMeshBuilderHelper.CalculateGridSize(Lod)];
            using var helper = new ChunkTerrainMeshBuilderHelper(_parent, Lod, grid);
            GetVerticalRange(VerticalIncrement, out var firstY, out var lastY);
            var sampleHeight = Math.Max(0, lastY - firstY + VerticalIncrement + 1);
            // Tall/cavernous chunks can need over 2 MB here, exceeding a worker's stack.
            var samples = ArrayPool<SampledBlock>.Shared.Rent(Math.Max(1, (BoundsX + 1) * sampleHeight * (BoundsZ + 1)));
            try
            {
                fixed (SampledBlock* cellSamples = samples)
                {
                    if (lastY >= firstY)
                        using (GenerationMetrics.Measure("mesh_sample_cache"))
                            helper.CacheCellSamples(cellSamples, firstY, lastY, HorizontalIncrement, VerticalIncrement, _useOptimizedTraversal);
                    IterateAndBuild(helper, ref failed, ProcessWater, ProcessColors, Cache, blockData, waterData,
                        HorizontalIncrement, VerticalIncrement, BuildIndexed);
                    PostProcessWater(Allocator, waterData, Lod, BuildIndexed);
                }
            }
            finally { ArrayPool<SampledBlock>.Shared.Return(samples); }

            return new ChunkMeshBuildOutput(blockData, waterData, new NativeVertexData(Allocator), failed);
        }

        private unsafe void IterateAndBuild(ChunkTerrainMeshBuilderHelper Helper, ref bool failed, bool ProcessWater,
            bool ProcessColors, RegionCache Cache, NativeVertexData blockData, NativeVertexData waterData,
            int HorizontalIncrement, int VerticalIncrement, bool BuildIndexed)
        {
            Loop(Helper, HorizontalIncrement, VerticalIncrement, ProcessColors, false, ref blockData, ref failed, ref Cache, BuildIndexed);
            //MarchingCubes.LoopSIMD(ref blockData, Helper._grid);
            if (ProcessWater && _parent.HasWater)
                Loop(Helper, 1, 1, ProcessColors, true, ref waterData, ref failed, ref Cache, BuildIndexed);
        }

        private void Loop(ChunkTerrainMeshBuilderHelper Helper, int HorizontalSkip, int VerticalSkip,
            bool ProcessColors, bool isWater, ref NativeVertexData blockData, ref bool failed, ref RegionCache Cache, bool BuildIndexed)
        {
            using var profile = GenerationMetrics.Measure("mesh_surface_loop");
            GetVerticalRange(VerticalSkip, out var firstY, out var lastY);
            if (failed || lastY < firstY) return;
            var vertexBuffer = MarchingCubes.NewVertexBuffer();
            var triangleBuffer = BuildIndexed ? null : MarchingCubes.NewTriangleBuffer();
            using var indexedBuffer = BuildIndexed ? new MarchingCubes.IndexedCellBuffer(firstY, lastY) : null;
            var vertexMap = BuildIndexed ? new Dictionary<Vector3, int>() : null;
            var isRiverConstant = _parent.IsRiverConstant;
            var cell = new GridCell
            {
                P = new Vector3[8],
                Type = new BlockType[8],
                Density = new float[8]
            };
            var width = BoundsX;
            var height = BoundsY;
            var depth = BoundsZ;
            if (isWater && _useOptimizedTraversal)
                using (GenerationMetrics.Measure("mesh_water_sample_cache")) Helper.CacheWaterSamples(firstY, lastY);
            for (var x = 0; x < width && !failed; x += HorizontalSkip)
            for (var y = firstY; y <= lastY && !failed; y += VerticalSkip)
            {
                var active = _useOptimizedTraversal ? (isWater ? Helper.GetWaterSurfaceMask(x, y)
                    : Helper.GetSurfaceMask(x, y, HorizontalSkip, VerticalSkip)) : 0;
                for (var z = 0; z < depth && !failed; z += HorizontalSkip)
                {
                    if (_useOptimizedTraversal)
                    {
                        if (active == 0) break;
                        z = BitOperations.TrailingZeroCount(active);
                        active &= active - 1;
                    }

                    if (!Helper.TryCreateCell(ref cell, x, y, z, isWater, HorizontalSkip, VerticalSkip, out var success)) continue;
                    if (!success && y < BoundsY - 2) failed = true;

                    var color = Vector4.One * 0.5f;
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

                    indexedBuffer?.PrepareCell(x, y, z);
                    PolygoniseCell(ref cell, ref blockData, ref vertexBuffer, ref triangleBuffer, color, ref isWater,
                        ref isRiverConstant, vertexMap, indexedBuffer);
                }
            }
        }

        private void GetVerticalRange(int VerticalStep, out int First, out int Last)
        {
            First = Math.Max(VerticalStep, ((Sparsity.MinimumHeight + VerticalStep - 1) / VerticalStep) * VerticalStep);
            Last = Math.Min(BoundsY - VerticalStep - 1, Sparsity.MaximumHeight);
        }
        
        private static void PolygoniseCell(ref GridCell Cell, ref NativeVertexData BlockData,
            ref Vector3[] VertexBuffer, ref Triangle[] TriangleBuffer, Vector4 Color, ref bool IsWater,
            ref bool IsRiverConstant, Dictionary<Vector3, int> VertexMap, MarchingCubes.IndexedCellBuffer IndexedBuffer)
        {
            if (VertexMap != null)
            {
                MarchingCubes.PolygoniseIndexedForRendering(ref Cell, VertexBuffer, BlockData, Color, IsWater, IsRiverConstant, VertexMap, IndexedBuffer);
                return;
            }
            MarchingCubes.Polygonise(ref Cell, ref VertexBuffer, ref TriangleBuffer, out var triangleCount);
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
