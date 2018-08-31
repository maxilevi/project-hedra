using Hedra.Engine.Player.Skills;
using Hedra.Engine.Rendering.UI;
using NUnit.Framework;
using OpenTK;

namespace HedraTests.Player.Skills
{
    [TestFixture]
    public class WarriorResistanceTest : SkillTest<WarriorResistance>
    {
        [Test]
        public void TestAddonHealthIsGiven()
        {
            var startingHealth = Player.AddonHealth;
            Skill.Level = 5;
            Skill.Update();
            Assert.AreNotEqual(startingHealth, Player.AddonHealth);
        }
        
        [Test]
        public void TestAddonHealthIsAddedWhenLoading()
        {
            var startingHealth = Player.AddonHealth;
            Skill.Level = 5;
            Skill.Update();
            Skill.Unload();
            Skill.Load();
            Assert.AreNotEqual(startingHealth, Player.AddonHealth);
        }
        
        [Test]
        public void TestAddonHealthIsRemovedWhenUnloading()
        {
            var startingHealth = Player.AddonHealth;
            Skill.Level = 5;
            Skill.Update();
            Skill.Unload();
            Assert.AreEqual(startingHealth, Player.AddonHealth);
        }
    }
}