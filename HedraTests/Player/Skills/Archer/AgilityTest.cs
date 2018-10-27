using Hedra.Engine.Player.Skills;
using Hedra.Engine.Player.Skills.Archer;
using NUnit.Framework;

namespace HedraTests.Player.Skills.Archer
{
    [TestFixture]
    public class AgilityTest : SkillTest<Agility>
    {
        [Test]
        public void TestDodgeCostIsReduced()
        {
            var originalCost = Player.DodgeCost;
            Skill.Level = 5;
            Skill.Update();
            Assert.AreNotEqual(originalCost, Player.DodgeCost);
        }
    }
}