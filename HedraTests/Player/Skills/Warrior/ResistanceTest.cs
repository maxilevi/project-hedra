using Hedra.Engine.SkillSystem.Warrior;
using NUnit.Framework;

namespace HedraTests.Player.Skills.Warrior
{
    [TestFixture]
    public class ResistanceTest : SkillTest<Resistance>
    {
        [Test]
        public void TestAddonHealthIsGiven()
        {
            var startingHealth = Player.BonusHealth;
            Skill.Level = 5;
            Skill.Update();
            Assert.AreNotEqual(startingHealth, Player.BonusHealth);
        }
        
        [Test]
        public void TestAddonHealthIsAddedWhenLoading()
        {
            var startingHealth = Player.BonusHealth;
            Skill.Level = 5;
            Skill.Update();
            Skill.Unload();
            Skill.Load();
            Assert.AreNotEqual(startingHealth, Player.BonusHealth);
        }
        
        [Test]
        public void TestAddonHealthIsRemovedWhenUnloading()
        {
            var startingHealth = Player.BonusHealth;
            Skill.Level = 5;
            Skill.Update();
            Skill.Unload();
            Assert.AreEqual(startingHealth, Player.BonusHealth);
        }
    }
}