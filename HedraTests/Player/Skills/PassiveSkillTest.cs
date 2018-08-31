using System;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Rendering.UI;
using Moq;
using NUnit.Framework;
using OpenTK;

namespace HedraTests.Player.Skills
{
    [TestFixture]
    public class PassiveSkillTest : SkillTest<PassiveSkillMock>
    {
        [Test]
        public void TestSkillDoesntSurpassMaxLevel()
        {
            Skill.Level = 0;
            Skill.SetMaxLevel(3);
            Skill.Level++;
            Skill.Update();
            Skill.Level++;
            Skill.Update();
            Skill.Level++;
            Skill.Update();
            Skill.Level++;
            Skill.Update();
            Skill.Level++;
            Skill.Update();
            Assert.AreEqual(3, Skill.Level);
        }
        
        [Test]
        public void TestOnChangeWasCalled()
        {
            var timesCalled = 0;
            Skill.OnChangeCallback = () => timesCalled++;
            Skill.Update();
            Skill.Update();
            Skill.Update();
            Assert.AreEqual(3, timesCalled);
        }
    }
}