using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Runtime.Serialization;
using Hedra;
using Hedra.BiomeSystem;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Moq;

namespace HedraTests.Rendering
{
    // CPU-only chunk inputs: no window, asset loading, renderer, actors, or game loop.
    public sealed class HeadlessTerrainFixture : IDisposable
    {
        private sealed class Provider : SimpleWorldProviderMock, IWorldProvider
        {
            public readonly Dictionary<Vector2, Chunk> Terrain = new();
            public bool MaterialNoise;
            float IWorldProvider.GetNoise(float X, float Y) => MaterialNoise ? (float)(Math.Sin(X * 7) * Math.Cos(Y * 9)) : 0;
            public override Dictionary<Vector2, Chunk> SearcheableChunks => Terrain;
            public override Chunk GetChunkByOffset(int X, int Z) => Terrain.TryGetValue(new Vector2(X, Z), out var chunk) ? chunk : null;
            public override Vector2 ToChunkSpace(Vector3 Position) => new(
                MathF.Floor(Position.X / Chunk.Width) * Chunk.Width, MathF.Floor(Position.Z / Chunk.Width) * Chunk.Width);
        }
        private readonly Hedra.Engine.Generation.IWorldProvider _previous;
        private readonly Random _previousRandom;
        public RegionCache Cache { get; }
        public Chunk Parent { get; }
        public ChunkTerrainMeshBuilder Builder { get; }
        public Block[] Blocks { get; }
        public ChunkMeshBuildOutput RawMesh(Hedra.Framework.IAllocator Allocator, int Lod, bool ProcessColors = false) =>
            (ChunkMeshBuildOutput)typeof(ChunkTerrainMeshBuilder).GetMethod("CreateTerrain", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(Builder, new object[] { Allocator, Lod, Cache, false, ProcessColors, 1, 1, false })!;
        public ChunkTerrainMeshBuilder DenseBuilder()
        {
            var builder = (ChunkTerrainMeshBuilder)Activator.CreateInstance(typeof(ChunkTerrainMeshBuilder),
                BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { Parent, false }, null)!;
            builder.Sparsity = Builder.Sparsity;
            return builder;
        }

        public HeadlessTerrainFixture(string Shape)
        {
            _previous = World.Provider;
            _previousRandom = Utils.Rng;
            var provider = new Provider { MaterialNoise = Shape == "materials" || Shape == "grass_palette" };
            var hasWater = Shape == "water" || Shape == "water_caves" || Shape == "high_water" || Shape == "water_missing";
            var region = new Region { Colors = new RegionColor(37)
            {
                GrassColors = new[] { new Vector4(.2f, .6f, .3f, 1) }, StoneColor = new Vector4(.4f, .35f, .3f, 1),
                DirtColor = new Vector4(.3f, .2f, .1f, 1), WaterColor = new Vector4(.1f, .4f, .8f, 1),
                SeafloorColor = new Vector4(.3f, .4f, .5f, 1)
            }};
            if (Shape == "materials" || Shape == "grass_palette") region.Colors.GrassColors = new[] {
                new Vector4(.13f, .37f, .17f, 1), new Vector4(.23f, .71f, .29f, 1), new Vector4(.17f, .53f, .41f, 1)
            };
            var biomes = new Mock<IBiomePool>();
            biomes.Setup(b => b.GetRegion(It.IsAny<Vector3>())).Returns(region);
            biomes.Setup(b => b.GetAverageRegionColor(It.IsAny<Vector3>())).Returns(region.Colors);
            provider.BiomePool = biomes.Object;
            World.Provider = provider;
            for (var cx = -1; cx <= 1; cx++)
            for (var cz = -1; cz <= 1; cz++)
            {
                var chunk = (Chunk)FormatterServices.GetUninitializedObject(typeof(Chunk));
                var landscape = (LandscapeGenerator)FormatterServices.GetUninitializedObject(typeof(LandscapeGenerator));
                Set(typeof(BiomeGenerator), landscape, "<BlocksSetted>k__BackingField", true);
                Set(typeof(Chunk), chunk, "<Landscape>k__BackingField", landscape);
                Set(typeof(Chunk), chunk, "<OffsetX>k__BackingField", cx * Chunk.Width);
                Set(typeof(Chunk), chunk, "<OffsetZ>k__BackingField", cz * Chunk.Width);
                Set(typeof(Chunk), chunk, "<Position>k__BackingField", new Vector3(cx * Chunk.Width, 0, cz * Chunk.Width));
                Set(typeof(Chunk), chunk, "_waterLock", new object());
                if (hasWater) Set(typeof(Chunk), chunk, "_waterPositions", Activator.CreateInstance(
                    typeof(Chunk).GetField("_waterPositions", BindingFlags.Instance | BindingFlags.NonPublic)!.FieldType));
                Set(typeof(Chunk), chunk, "<IsRiverConstant>k__BackingField", true);
                var blocks = new Block[Chunk.BoundsX * Chunk.BoundsY * Chunk.BoundsZ];
                for (var x = 0; x < Chunk.BoundsX; x++)
                for (var y = 0; y < Chunk.BoundsY; y++)
                for (var z = 0; z < Chunk.BoundsZ; z++)
                {
                    var wx = x + cx * Chunk.BoundsX;
                    var wz = z + cz * Chunk.BoundsZ;
                    var height = Shape == "flat" ? 65.3f : (Shape == "tall_caves" ? 225.3f : Shape == "high_water" ? 235.3f : 68.3f)
                        + 14 * MathF.Sin(wx * .13f) * MathF.Cos(wz * .17f);
                    var density = height - y;
                    if (Shape == "caves" || Shape == "tall_caves" || Shape == "water_caves") density = MathF.Min(density,
                        (MathF.Abs(MathF.Sin(wx * .21f) + MathF.Sin(y * .19f) + MathF.Cos(wz * .23f)) - .4f) * 8);
                    if (Shape == "water_missing") density = -1;
                    var type = density > 0 ? BlockType.Grass : hasWater && y < (Shape == "high_water" ? 256 : 73)
                        ? BlockType.Water : BlockType.Air;
                    if (Shape == "materials" && density > 0)
                        type = height - y < 2 ? (wx % 5 == 0 ? BlockType.StonePath : BlockType.Grass)
                            : height - y < 7 ? BlockType.Dirt : BlockType.Stone;
                    blocks[x * Chunk.BoundsY * Chunk.BoundsZ + y * Chunk.BoundsZ + z] = new Block(type, density);
                }
                Set(typeof(Chunk), chunk, "_blocks", blocks);
                provider.Terrain.Add(new Vector2(cx * Chunk.Width, cz * Chunk.Width), chunk);
                if (cx == 0 && cz == 0) { Parent = chunk; Blocks = blocks; }
            }
            if (Shape == "water_missing") provider.Terrain.Remove(new Vector2(Chunk.Width, 0));
            Cache = new RegionCache(Vector3.Zero, new Vector3(Chunk.Width, 0, Chunk.Width));
            Builder = new ChunkTerrainMeshBuilder(Parent) { Sparsity = ChunkSparsity.From(Parent) };
        }

        public static void Set(Type Type, object Target, string Field, object Value) =>
            Type.GetField(Field, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(Target, Value);
        public void Dispose()
        {
            ChunkTerrainMeshBuilder.ClearMapping(Vector3.Zero);
            World.Provider = _previous;
            Utils.Rng = _previousRandom;
        }
    }
}
