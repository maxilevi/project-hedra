using Hedra.Engine.Events;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using NUnit.Framework;
using OpenTK;

namespace HedraTests.WorldBuilding
{
    [TestFixture]
    public class ObeliskTest : BaseTest
    {
        private Obelisk _obelisk;
        private PlayerMock _dummyPlayer;
        
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _obelisk = new Obelisk();
            _dummyPlayer = new PlayerMock();
            Assert.Null(_dummyPlayer.MessageMock.LastMessage);
        }
        
        [Test]
        public void TestObeliskCanOnlyBeUsedOnce()
        {
            _obelisk.Type = ObeliskType.Xp;
            
            var startXp = _dummyPlayer.XP;
            _obelisk.InvokeInteraction(_dummyPlayer);
            Assert.True(_obelisk.Disposed);
            Assert.True(_obelisk.Interacted);
        }
        
        [Test]
        public void TestObeliskXp()
        {
            _obelisk.Type = ObeliskType.Xp;
            
            var startXp = _dummyPlayer.XP;
            _obelisk.InvokeInteraction(_dummyPlayer);
            Assert.NotNull(_dummyPlayer.MessageMock.LastMessage);
            Assert.Greater(_dummyPlayer.XP, startXp);
        }
        
        [Test]
        public void TestObeliskHealth()
        {
            _obelisk.Type = ObeliskType.Health;         
            var startHealth = _dummyPlayer.Health;
            _obelisk.InvokeInteraction(_dummyPlayer);
            Assert.NotNull(_dummyPlayer.MessageMock.LastMessage);
            Assert.Greater(_dummyPlayer.Health, startHealth);
        }
        
        [Test]
        public void TestObeliskMana()
        {
            _obelisk.Type = ObeliskType.Mana;     
            var startMana = _dummyPlayer.Mana;
            _obelisk.InvokeInteraction(_dummyPlayer);
            Assert.NotNull(_dummyPlayer.MessageMock.LastMessage);
            Assert.Greater(_dummyPlayer.Mana, startMana);
        }
        
        [Test]
        public void TestObeliskMobs()
        {
            _obelisk.Type = ObeliskType.Mobs;
            var provider = new WorldSpawnMock();
            World.Provider = provider;
            Assert.AreEqual(provider.MobsSpawned, 0);
            
            _obelisk.InvokeInteraction(_dummyPlayer);
            Assert.NotNull(_dummyPlayer.MessageMock.LastMessage);
            Assert.Greater(provider.MobsSpawned, 0);
        }
        
        [Test]
        public void TestObeliskColorsExist()
        {
            for (var i = 0; i < (int) ObeliskType.MaxItems; i++)
            {
                Assert.DoesNotThrow(() =>
                    Assert.NotNull(Obelisk.GetObeliskColor((ObeliskType) i))
                );
            }
        }
    }
}