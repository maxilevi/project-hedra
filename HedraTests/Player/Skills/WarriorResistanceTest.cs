using Hedra.Engine.Player.Skills;
using Hedra.Engine.Rendering.UI;
using NUnit.Framework;
using OpenTK;

namespace HedraTests.Player.Skills
{
    [TestFixture]
    public class WarriorResistanceTest : SkillTest
    {
        private WarriorResistance _skill;
        
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _skill = new WarriorResistance();
            _skill.Initialize(Vector2.Zero, Vector2.One, new Panel(), Player);
        }

        [Test]
        public void TestAddonHealthIsGiven()
        {
            var startingHealth = Player.AddonHealth;
            _skill.Level = 5;
            _skill.Update();
            Assert.AreNotEqual(startingHealth, Player.AddonHealth);
        }
        
        [Test]
        public void TestAddonHealthIsRemovedWhenLoading()
        {
            
        }
        
        [Test]
        public void TestAddonHealthIsRemovedWhenUnloading()
        {
            
        }
    }
}