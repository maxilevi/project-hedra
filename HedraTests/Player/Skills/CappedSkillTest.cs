using NUnit.Framework;

namespace HedraTests.Player.Skills
{
    [TestFixture]
    public class CappedSkillTest : SkillTest<CappedSkillMock>
    {
        [Test]
        public void TestSkillLevelDoesntExceedCap()
        {
            Skill.SetMaxLevel(5);
            Skill.Level = 10;
            Skill.Update();
            Assert.AreEqual(5, Skill.Level);
        }
    }
}