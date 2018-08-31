using NUnit.Framework;

namespace HedraTests.Player.Skills
{
    [TestFixture]
    public class LearningSkillTest : SkillTest<LearningSkillMock>
    {
        [Test]
        public void TestLearnIsExecuted()
        {
            var calledTimes = 0;
            Skill.OnLearnCallback += () => calledTimes++;
            Skill.Level = 1;
            Skill.Update();
            Assert.AreEqual(1, calledTimes);
        }
        
        [Test]
        public void TestMaxLevelIs1()
        {
            Skill.Level = 5;
            Skill.Update();
            Assert.AreEqual(1, Skill.Level);
        }
    }
}