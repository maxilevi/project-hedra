using Hedra.Engine.SkillSystem.Rogue;
using NUnit.Framework;

namespace HedraTests.Player.Skills.Rogue
{
    [TestFixture]
    public class BurstOfSpeedTest : SkillTest<BurstOfSpeed>
    {
        [Test]
        public void TestBurstIsGiven()
        {
            var startingSpeed = Player.Speed;
            Skill.Level = 1;
            Skill.Update();
            Skill.Use();
            Assert.AreNotEqual(startingSpeed, Player.Speed);
        }
    }
}