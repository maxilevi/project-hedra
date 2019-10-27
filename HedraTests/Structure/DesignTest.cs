using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.WorldBuilding;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using Hedra;
using Hedra.AISystem;
using Hedra.BiomeSystem;
using Hedra.Components;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.BiomeSystem.NormalBiome;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player.MapSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Rendering;
using Hedra.Structures;
using HedraTests.Player;
using Moq;
using System.Numerics;
using Hedra.Numerics;

namespace HedraTests.Structure
{
    public abstract class DesignTest<T> : BaseTest where T : StructureDesign, new()
    {
        private Random _rng;
        private List<IEntity> _interceptedEntities;
        private StructureDesign[] _designs;
        private BiomeStructureDesign _biomeStructureDesign;
        
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            DesiredHeight = BiomePool.SeaLevel+1;
            NameGenerator.Load();
            GameManager.Player = new PlayerMock();
            var cacheProvider = new Mock<ICacheProvider>();
            var defaultShape = new List<CollisionShape>();
            defaultShape.Add(new CollisionShape(VertexData.Empty));
            cacheProvider.Setup(C => C.GetModel(It.IsAny<string>())).Returns(new VertexData());
            cacheProvider.Setup(C => C.GetShape(It.IsAny<string>(), It.IsAny<VertexData>()))
                .Returns(defaultShape);
            cacheProvider.Setup(C => C.GetShape(It.IsAny<VertexData>()))
                .Returns(defaultShape);
            CacheManager.Provider = cacheProvider.Object;
            _interceptedEntities = new List<IEntity>();
            var defaultRegion = new Region();
            defaultRegion.Colors = new RegionColor(1, new NormalBiomeColors());
            defaultRegion.Generation = new RegionGeneration(1, new SimpleGenerationDesignMock(() => DesiredHeight));
            var structureDesignMock = new Mock<BiomeStructureDesign>();
            structureDesignMock.Setup(S => S.Designs).Returns(() => _designs);
            _biomeStructureDesign = structureDesignMock.Object;
            defaultRegion.Structures = new RegionStructure(1, _biomeStructureDesign);
            var biomePoolMock = new Mock<IBiomePool>();
            biomePoolMock.Setup(B => B.GetRegion(It.IsAny<Vector3>())).Returns(defaultRegion);
            var worldMock = new Mock<IWorldProvider>();
            var normalProvider = new WorldProvider();
            worldMock.Setup(W => W.ToChunkSpace(It.IsAny<Vector3>())).Returns<Vector3>(V => normalProvider.ToChunkSpace(V));
            worldMock.Setup(W => W.BiomePool).Returns(biomePoolMock.Object);
            worldMock.Setup(W => W.AddEntity(It.IsAny<IEntity>())).Callback(delegate(IEntity Entity)
            {
                if(!_interceptedEntities.Contains(Entity)) _interceptedEntities.Add(Entity);
            });
            var guardAiComponentMock = new Mock<IGuardAIComponent>();
            guardAiComponentMock.As<ITraverseAIComponent>();
            worldMock.Setup(W => W.SpawnMob(It.IsAny<string>(), It.IsAny<Vector3>(), It.IsAny<int>())).Returns(delegate
            {
                var ent = new SkilledAnimableEntity();
                ent.AddComponent(new HealthBarComponent(ent, "test", HealthBarType.Hostile));
                ent.AddComponent(new DamageComponent(ent));
                ent.AddComponent(guardAiComponentMock.Object);
                if(!_interceptedEntities.Contains(ent)) _interceptedEntities.Add(ent);
                return ent;
            });
            worldMock.Setup(W => W.WorldBuilding).Returns(new StructureDesignWorldBuildingMock());
            worldMock.Setup(W => W.StructureHandler).Returns(new StructureHandler());
            World.Provider = worldMock.Object;
            Design = new T();
            _rng = new Random();
            _designs = new NormalBiomeStructureDesign().Designs;
        }
        
        [Test]
        public void TestDesignSpawnsWithinRange()
        {
            var structure = this.CreateStructure();
            /* Villages can have farms outside the main plateau */
            var multiplier = structure.Design is VillageDesign ? 1.5f : 1.0f;
            Design.Build(structure);
            Executer.Update();
            for (var i = 0; i < WorldEntities.Length; i++)
            {
                if( (WorldEntities[i].Position.Xz() - structure.Position.Xz()).LengthFast() > Design.PlateauRadius * multiplier)
                    Assert.Fail($"{WorldEntities[i].Position.Xz()} is far from {structure.Position.Xz()} by more than {(WorldEntities[i].Position.Xz() - structure.Position.Xz()).LengthFast()}");
            }

            var structures = GetStructureObjects(structure);
            for (var i = 0; i < structures.Length; i++)
            {
                if( (structures[i].Position.Xz() - structure.Position.Xz()).LengthFast() > Design.PlateauRadius * multiplier)
                    Assert.Fail($"'{structures[i]}': {structures[i].Position.Xz()} is far from {structure.Position.Xz()} by more than {(WorldEntities[i].Position.Xz() - structure.Position.Xz()).LengthFast()}");
            }
        }
        
        [Test]
        public void TestDesignSpawnsEntitiesOrStructures()
        {
            var structure = this.CreateStructure();
            Design.Build(structure);
            Executer.Update();
            Assert.Greater(WorldEntities.Length + GetStructureObjects(structure).Length, 0);
        }

        [Test]
        public void TestDesignPlacementMatchesWithMapPlacement()
        {
            var design = new T();
            SetDesigns(design);
            var currentOffset = World.ToChunkSpace(new Vector3(Utils.Rng.Next(int.MinValue, int.MaxValue), 0, Utils.Rng.Next(int.MinValue, int.MaxValue)));
            var distribution = new RandomDistribution();
            for (var x = Math.Min(-2, -design.PlateauRadius / Chunk.Width * 2); x < Math.Max(2, design.PlateauRadius / Chunk.Width * 2); x++)
            {
                for (var z = Math.Min(-2, -design.PlateauRadius / Chunk.Width * 2); z < Math.Max(2, design.PlateauRadius / Chunk.Width * 2); z++)
                {
                    var offset = new Vector2(currentOffset.X + x * Chunk.Width, currentOffset.Y + z * Chunk.Width);
                    distribution.Seed = StructureDesign.BuildRngSeed(offset);
                    var targetPosition = StructureDesign.BuildTargetPosition(offset, distribution);
                    var biome = World.BiomePool.GetRegion(targetPosition);
                    Assert.AreEqual(
                        MapBuilder.Sample(offset.ToVector3(), biome) != null,
                        design.ShouldSetup(offset, ref targetPosition, World.StructureHandler.StructureItems,biome, distribution)
                    );
                }
            }
        }

        private void SetDesigns(params StructureDesign[] Designs)
        {
            _designs = Designs;
            // Get the mocked region.
            World.BiomePool.GetRegion(Vector3.Zero).Structures = new RegionStructure(1, _biomeStructureDesign);
        }
        /*
        [Test]
        public void TestDesignPlacement()
        {
            var design = new T();
            _designs = new StructureDesign[] {new T()};
            var currentOffset = World.ToChunkSpace(new Vector3(Utils.Rng.Next(int.MinValue, int.MaxValue), 0, Utils.Rng.Next(int.MinValue, int.MaxValue)));
            var distribution = new RandomDistribution();
            for (var x = Math.Min(-2, -design.Radius / Chunk.Width * 2); x < Math.Max(2, design.Radius / Chunk.Width * 2); x++)
            {
                for (var z = Math.Min(-2, -design.Radius / Chunk.Width * 2); z < Math.Max(2, design.Radius / Chunk.Width * 2); z++)
                {
                    var offset = new Vector2(currentOffset.X + x * Chunk.Width, currentOffset.Y + z * Chunk.Width);
                    distribution.Seed = StructureDesign.BuildRngSeed(offset);
                    var targetPosition = StructureDesign.BuildTargetPosition(offset, distribution);
                    var biome = World.BiomePool.GetRegion(targetPosition);
                    Assert.AreEqual(
                        MapBuilder.Sample(offset.ToVector3(), biome),
                        design.ShouldSetup(offset, targetPosition, World.StructureHandler.StructureItems,
                            biome, distribution)
                    );
                }
            }
        }*/

        protected BaseStructure[] GetStructureObjects(CollidableStructure Structure)
        {
            var list = new List<BaseStructure>();
            if (Structure.WorldObject != null)
            {
                list.Add(Structure.WorldObject);

                void RecursiveSelect(BaseStructure S, List<BaseStructure> List)
                {
                    var children = S.Children;
                    foreach (var child in children)
                    {
                        List.Add(child);
                        RecursiveSelect(child, List);
                    }
                }
                RecursiveSelect(Structure.WorldObject, list);
            }
            return list.ToArray();
        }
        
        protected virtual CollidableStructure CreateStructure()
        {
            var location = RandomLocation;
            return new CollidableStructure(Design, location, new RoundedPlateau(location.Xz(), _rng.Next(32, 1024)), CreateBaseStructure(location));
        }

        protected abstract BaseStructure CreateBaseStructure(Vector3 Position);
        
        protected float DesiredHeight { get; set; }
        protected IEntity[] WorldEntities => _interceptedEntities.ToArray();
        protected T Design { get; private set; }
        protected Vector3 RandomLocation => 
            new Vector3(_rng.NextFloat() * 8000 - 4000, 200, _rng.NextFloat() * 8000 - 4000);
    }
}