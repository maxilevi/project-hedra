using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.WorldBuilding;
using NUnit.Framework;
using System.Collections.Generic;
using Hedra.Engine;
using Hedra.Engine.AISystem;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.BiomeSystem.NormalBiome;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using HedraTests.Player;
using Moq;
using OpenTK;

namespace HedraTests.Structure
{
    public abstract class DesignTest<T> : BaseTest where T : StructureDesign, new()
    {
        private Random _rng;
        private List<IEntity> _interceptedEntities;
        private List<BaseStructure> _interceptedStructures;
        
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
            _interceptedStructures = new List<BaseStructure>();
            var defaultRegion = new Region();
            defaultRegion.Colors = new RegionColor(1, new NormalBiomeColors());
            defaultRegion.Generation = new RegionGeneration(1, new SimpleGenerationDesignMock(() => DesiredHeight));
            var biomePoolMock = new Mock<IBiomePool>();
            biomePoolMock.Setup(B => B.GetRegion(It.IsAny<Vector3>())).Returns(defaultRegion);
            var worldMock = new Mock<IWorldProvider>();
            worldMock.Setup(W => W.BiomePool).Returns(biomePoolMock.Object);
            worldMock.Setup(W => W.AddEntity(It.IsAny<IEntity>())).Callback(delegate(IEntity Entity)
            {
                if(!_interceptedEntities.Contains(Entity)) _interceptedEntities.Add(Entity);
            });
            worldMock.Setup(W => W.AddStructure(It.IsAny<BaseStructure>())).Callback(delegate(BaseStructure Structure)
            {
                _interceptedStructures.Add(Structure);
            });
            var guardAiComponentMock = new Mock<IGuardAIComponent>();
            worldMock.Setup(W => W.SpawnMob(It.IsAny<string>(), It.IsAny<Vector3>(), It.IsAny<int>())).Returns(delegate
            {
                var ent = new Entity();
                ent.AddComponent(new HealthBarComponent(ent, "test"));
                ent.AddComponent(new DamageComponent(ent));
                ent.AddComponent(guardAiComponentMock.Object);
                if(!_interceptedEntities.Contains(ent)) _interceptedEntities.Add(ent);
                return ent;
            });
            worldMock.Setup(W => W.WorldBuilding).Returns(new StructureDesignWorldBuildingMock());
            World.Provider = worldMock.Object;
            Design = new T();
            _rng = new Random();
        }
        
        [Test]
        public void TestDesignSpawnsWithinRange()
        {
            var structure = this.CreateStructure();
            Design.Build(structure);
            Executer.Update();
            for (var i = 0; i < WorldEntities.Length; i++)
            {
                if( (WorldEntities[i].BlockPosition.Xz - structure.Position.Xz).LengthFast > Design.Radius )
                    Assert.Fail($"{WorldEntities[i].Position.Xz} is far from {structure.Position.Xz} by more than {Design.Radius}");
            }
            for (var i = 0; i < WorldStructures.Length; i++)
            {
                if( (WorldStructures[i].Position.Xz - structure.Position.Xz).LengthFast > Design.Radius )
                    Assert.Fail($"{WorldStructures[i].Position.Xz} is far from {structure.Position.Xz} by more than {Design.Radius}");
            }
        }
        
        [Test]
        public void TestDesignSpawnsEntitiesOrStructures()
        {
            var structure = this.CreateStructure();
            Design.Build(structure);
            Executer.Update();
            Assert.Greater(WorldEntities.Length + WorldStructures.Length, 0);
        }

        protected virtual CollidableStructure CreateStructure()
        {
            return new CollidableStructure(Design, RandomLocation, null);
        }
        
        protected float DesiredHeight { get; set; }
        protected IEntity[] WorldEntities => _interceptedEntities.ToArray();
        protected BaseStructure[] WorldStructures => _interceptedStructures.ToArray();
        protected T Design { get; private set; }
        protected Vector3 RandomLocation => 
            new Vector3(_rng.NextFloat() * 8000 - 4000, 200, _rng.NextFloat() * 8000 - 4000);
    }
}