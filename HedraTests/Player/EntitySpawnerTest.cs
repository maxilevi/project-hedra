using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Threading;
using Hedra;
using Hedra.BiomeSystem;
using Hedra.Engine;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.BiomeSystem.NormalBiome;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.EntitySystem;
using Hedra.Game;
using HedraTests.Structure;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace HedraTests.Player
{
    [TestFixture]
    public class EntitySpawnerTest : BaseTest
    {
        private PlayerMock _player;
        private MockEntitySpawner _spawner;
        private event EventHandler OnSpawnCallback;
        private Block _spawningBlock;
        private int _currentHeight;
        private SimpleGameProviderMock _provider;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _provider = (SimpleGameProviderMock) GameManager.Provider;
            _provider.Exists = false;
            _player = new PlayerMock();
            GameManager.Player = _player;
            _spawningBlock = new Block
            {
                Type = BlockType.Grass
            };
            var entities = new List<IEntity>();
            _spawner = new MockEntitySpawner(_player)
            {
                Enabled = true,
                SpawnChance = 1
            };
            var worldMock = new Mock<IWorldProvider>();
            worldMock.Setup(W => W.SpawnMob(It.IsAny<string>(), It.IsAny<Vector3>(), It.IsAny<int>()))
                .Callback(delegate
                {
                    OnSpawnCallback?.Invoke(null, null);
                    var mockEntity = new Mock<IEntity>();
                    mockEntity.Setup(E => E.IsStatic).Returns(false);
                    entities.Add(mockEntity.Object);
                });
            worldMock.Setup(W => W.Entities).Returns( () => new ReadOnlyCollection<IEntity>(entities));
            worldMock.Setup(W => W.GetHighestY(It.IsAny<int>(), It.IsAny<int>())).Returns( () => _currentHeight);
            worldMock.Setup(W => W.GetHighestBlockAt(It.IsAny<int>(), It.IsAny<int>())).Returns( () => _spawningBlock);
            var defaultRegion = new Region();
            defaultRegion.Colors = new RegionColor(1, new NormalBiomeColors());
            defaultRegion.Generation = new RegionGeneration(1, new SimpleGenerationDesignMock(() => 50));
            var biomePoolMock = new Mock<IBiomePool>();
            biomePoolMock.Setup(B => B.GetRegion(It.IsAny<Vector3>())).Returns(defaultRegion);
            worldMock.Setup(W => W.BiomePool).Returns(biomePoolMock.Object);
            World.Provider = worldMock.Object;
            _currentHeight = 1;
        }
        
        //TODO: Test EntitySpawner::SelectTemplate()

        [Test]
        public void TestCanSpawnMobs()
        {
            this.AssertMobsSpawned(MobType.Sheep, 1, 1);
        }
        
        [Test]
        public void TestCanSpawnMobsInGroups()
        {
            this.AssertMobsSpawned(MobType.Sheep, 8, 8);
        }
        
        [Test]
        public void TestMobsDontSpawnOnInvalidBlocks()
        {
            _spawningBlock = new Block(BlockType.Water);
            this.AssertMobsSpawned(MobType.Sheep, 1, 0);
            _spawningBlock = new Block(BlockType.Seafloor);
            this.AssertMobsSpawned(MobType.Sheep, 1, 0);
            _spawningBlock = new Block(BlockType.Air);
            this.AssertMobsSpawned(MobType.Sheep, 1, 0);
        }
        
        [Test]
        public void TestMobsCantSpawnUnderSeaLevel()
        {
            _currentHeight = -1;
            this.AssertMobsSpawned(MobType.Sheep, 1, 0);
        }
        
        [Test]
        public void TestSpawnerCantExceedMobCap()
        {
            EntitySpawner.MobCap = 2;
            this.AssertMobsSpawned(MobType.Sheep, 1, 1);
            this.AssertMobsSpawned(MobType.Sheep, 1, 1);
            this.AssertMobsSpawned(MobType.Sheep, 1, 0);
        }
        
        [Test]
        public void TestSpawningThreadIsLaunched()
        {
            var pause = new ManualResetEvent(false);
            _provider.Exists = true;
            var spawnerMock = new Mock<EntitySpawner>(_player);
            spawnerMock.Setup(S => S.Update()).Callback(delegate
            {
                _provider.Exists = false;
                pause.Set();
            });
            var spawner = spawnerMock.Object;
            Assert.True(pause.WaitOne(100), "Failed to dispatch thread when starting the entity spawner.");
        }
        
        [Test]
        public void TestSpawnerSpawnChance()
        {
            _spawner.SpawnChance = 0;
            this.AssertMobsSpawned(MobType.Sheep, 1, 0);
        }

        private void AssertMobsSpawned(MobType Type, int Input, int Expected)
        {
            var mobsSpawned = 0;
            OnSpawnCallback += (Sender, Args) =>
            {
                mobsSpawned++;
            };
            this.Spawn(Type, Input);
            Assert.AreEqual(Expected, mobsSpawned);
        }
        
        private void Spawn(MobType Type, int Count)
        {
            _spawner.TargetTemplate = new SpawnTemplate
            {
                Type = Type.ToString(),
                MinGroup = Count,
                MaxGroup = Count
            };
            _spawner.Update();
        }

        [TearDown]
        public override void Teardown()
        {
            base.Teardown();
            var invocationList = OnSpawnCallback?.GetInvocationList();
            if (invocationList == null) return;
            for (var i = 0; i < invocationList.Length; i++)
            {
                OnSpawnCallback -= (EventHandler) invocationList[i];
            }
        }
    }
}