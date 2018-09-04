using Hedra.Engine.Player.Skills;
using Hedra.Engine.Player.Skills.Warrior;
using NUnit.Framework;

namespace HedraTests.Player.Skills.Warrior
{
    [TestFixture]
    public class ResistanceTest : SkillTest<Resistance>
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