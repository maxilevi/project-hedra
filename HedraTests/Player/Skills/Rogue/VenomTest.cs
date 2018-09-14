using Hedra.Engine.Player.Skills.Rogue;
using NUnit.Framework;

namespace HedraTests.Player.Skills.Rogue
{
    [TestFixture]
    public class VenomTest : SkillTest<Venom>
    {

        [Test]
        public void TestVenomGivesComponent()
        {
            Skill.Level = 1;
            Skill.Update();
        }

        [Test]
        public void TestVenomRemovesComponent()
        {
            
        }

        [Test]
        public void TestVenomComponentScales()
        {
            
        }

        [Test]
        public void TestComponentIsGivenEvenIfRemoved()
        {
            
        }
    }
}