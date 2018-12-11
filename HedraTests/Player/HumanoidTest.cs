using Hedra.Engine.ModuleSystem;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.Engine.Player;
using NUnit.Framework;

namespace HedraTests.Player
{
    [TestFixture]
    public class HumanoidTest : BaseTest
    {
        private Humanoid _human;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _human = new Humanoid();
            _human.Model = new HumanoidModel(_human, new HumanoidModelTemplate
            {
                Path = string.Empty,
                Scale = 0
            });
        }

        [Test]
        public void TestConsecutiveHitsAccumulate()
        {
            _human.ProcessHit(true);
            _human.ProcessHit(true);
            _human.ProcessHit(true);
            _human.ProcessHit(true);
            _human.ProcessHit(true);
            Assert.AreEqual(5, _human.ConsecutiveHits);
            Assert.Greater(_human.ConsecutiveHitsModifier, 0);
        }

        [Test]
        public void TestConsecutiveHitsAreLostWhenMisses()
        {
            _human.ProcessHit(true);
            _human.ProcessHit(true);
            Assert.AreEqual(2, _human.ConsecutiveHits);
            _human.ProcessHit(false);
            Assert.AreEqual(0, _human.ConsecutiveHits);
        }

        [Test]
        public void TestExtraHealthRegenWhenSleeping()
        {
            var noSleepingRegen = _human.HealthRegen;
            _human.IsSleeping = true;
            Assert.Greater(_human.HealthRegen, noSleepingRegen);
        }
        
        [Test]
        public void TestExtraManaRegenWhenSleeping()
        {
            var noSleepingRegen = _human.ManaRegen;
            _human.IsSleeping = true;
            Assert.Greater(_human.ManaRegen, noSleepingRegen);
        }
        
        [Test]
        public void TestMaxLevelCantBeSurpassed()
        {
            _human.Level = 0;
            _human.XP += RecursiveXp(Humanoid.MaxLevel + 10);
            Assert.AreEqual(0, _human.MaxXP);
            Assert.AreEqual(0, _human.XP);
            Assert.AreEqual(Humanoid.MaxLevel, _human.Level);
        }
        
        [Test]
        public void TestHumanoidLevelsUp()
        {
            _human.Level = 1;
            _human.XP += _human.MaxXP;
            Assert.AreEqual(2, _human.Level);
        }
        
        [Test]
        public void TestHumanoidXpLoopsUntilLevel()
        {
            _human.Level = 0;
            _human.XP += RecursiveXp(10);
            Assert.AreEqual(10, _human.Level);
        }
        
        [Test]
        public void TestHumanoidXpIsAdded()
        {
            _human.Level = 1;
            _human.XP = 5;
            Assert.AreEqual(5, _human.XP);
        }

        private float RecursiveXp(int Target)
        {
            if (Target == 0) return 0;
            return _human.Class.XPFormula(Target) + RecursiveXp(Target - 1);
        }
    }
}