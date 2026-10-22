using System;
using System.Numerics;
using System.Collections.Generic;
using System.Reflection;
using Hedra.Engine.WorldBuilding;
using System.Runtime.Serialization;
using Hedra;
using Hedra.BiomeSystem;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.BiomeSystem.NormalBiome;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Game;
using HedraTests.Rendering;
using Moq;

namespace HedraTests.Generation
{
    public sealed class HeadlessGenerationFixture : IDisposable
    {
        private sealed class Provider : SimpleWorldProviderMock, IWorldProvider
        {
            private readonly int _seed;
            private readonly bool _features;
            public Provider(int Seed, bool Features) { _seed = Seed; _features = Features; }
            float IWorldProvider.GetNoise(float X, float Y) => _features ? (float)(Math.Sin(X * .7) * Math.Cos(Y * .9)) : 0;
            public override int Seed => _seed;
        }
        private sealed class Generator : LandscapeGenerator
        {
            public Generator(Chunk Chunk) : base(Chunk) { }
            public ChunkDetails Generate(Block[] Blocks)
            {
                Noise.Seed = Seed;
                Array.Clear(Blocks, 0, Blocks.Length);
                HeadlessTerrainFixture.Set(typeof(Chunk), Parent, "_waterPositions", null);
                return DefineBlocks(Blocks);
            }
        }

        private readonly IWorldProvider _world;
        private readonly IGameProvider _game;
        private readonly RegionGeneration _region;
        private readonly Generator _generator;
        public Block[] Blocks { get; } = new Block[Chunk.BoundsX * Chunk.BoundsY * Chunk.BoundsZ];

        private readonly Chunk _chunk;
        public CoordinateHash3D[] WaterPositions => ((List<CoordinateHash3D>)typeof(Chunk)
            .GetField("_waterPositions", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_chunk))?.ToArray()
            ?? Array.Empty<CoordinateHash3D>();

        public HeadlessGenerationFixture(int Seed, int X, int Z, bool Features = false, BiomeGenerationDesign Design = null)
        {
            _world = World.Provider;
            _game = GameManager.Provider;
            GameManager.Provider = new SimpleGameProviderMock();
            var provider = new Provider(Seed, Features) { WorldBuilding = new Hedra.Engine.WorldBuilding.WorldBuilding() };
            World.Provider = provider;
            _region = new RegionGeneration(Seed, Design ?? new NormalBiomeGenerationDesign());
            var region = new Region { Generation = _region };
            var biomes = new Mock<IBiomePool>();
            biomes.Setup(b => b.GetRegion(It.IsAny<Vector3>())).Returns(region);
            provider.BiomePool = biomes.Object;
            if (Features)
            {
                var center = new Vector2(X + 64, Z + 64);
                // Its constructor samples structures/assets to choose an initial
                // height. Supply that height directly; exercise real Apply/Density.
                var plateau = (RoundedPlateau)FormatterServices.GetUninitializedObject(typeof(RoundedPlateau));
                HeadlessTerrainFixture.Set(typeof(BasePlateau), plateau, "<Position>k__BackingField", center);
                plateau.Radius = 80;
                plateau.Hardness = 3;
                plateau.MaxHeight = 53;
                var plateaus = new BasePlateau[] { plateau };
                var groundworks = new IGroundwork[] {
                    new RoundedGroundwork(new Vector3(X + 32, 0, Z + 32), 38, BlockType.Grass) { BonusHeight = 4 },
                    new RoundedGroundwork(new Vector3(X + 80, 0, Z + 80), 30, BlockType.FarmDirt) { BonusHeight = 2 },
                    new RoundedGroundwork(new Vector3(X + 110, 0, Z + 20), 25, BlockType.Air)
                };
                var building = new Mock<IWorldBuilding>();
                building.Setup(b => b.GetPlateausFor(It.IsAny<Vector2>())).Returns(plateaus);
                building.Setup(b => b.GetLandformsFor(It.IsAny<Vector2>())).Returns(Array.Empty<Landform>());
                building.Setup(b => b.GetGroundworksFor(It.IsAny<Vector2>())).Returns(groundworks);
                provider.WorldBuilding = building.Object;
            }
            var chunk = (Chunk)FormatterServices.GetUninitializedObject(typeof(Chunk));
            HeadlessTerrainFixture.Set(typeof(Chunk), chunk, "<OffsetX>k__BackingField", X);
            HeadlessTerrainFixture.Set(typeof(Chunk), chunk, "<OffsetZ>k__BackingField", Z);
            HeadlessTerrainFixture.Set(typeof(Chunk), chunk, "<Position>k__BackingField", new Vector3(X, 0, Z));
            HeadlessTerrainFixture.Set(typeof(Chunk), chunk, "<Biome>k__BackingField", region);
            HeadlessTerrainFixture.Set(typeof(Chunk), chunk, "_blocks", Blocks);
            HeadlessTerrainFixture.Set(typeof(Chunk), chunk, "_waterLock", new object());
            _chunk = chunk;
            _generator = new Generator(chunk);
            HeadlessTerrainFixture.Set(typeof(Chunk), chunk, "<Landscape>k__BackingField", _generator);
        }

        public ChunkDetails Generate() => _generator.Generate(Blocks);
        public void Dispose()
        {
            _generator.Dispose();
            _region.Dispose();
            World.Provider = _world;
            GameManager.Provider = _game;
        }
    }
}
