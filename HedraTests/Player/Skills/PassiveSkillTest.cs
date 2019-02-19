using System;
using Hedra.Engine.Player.AbilityTreeSystem;
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
            Skill.Level = 1;
            Skill.SetMaxLevel(1);
            Skill.OnAddCallback = () => timesCalled++;
            Skill.Update();
            Skill.Update();
            Skill.Update();
            Assert.AreEqual(3, timesCalled);
        }
        
        [Test]
        public void TestOnChangeIsNotCalledIfDoesntHaveSkill()
        {
            var timesCalled = 0;
            Skill.OnAddCallback = () => timesCalled++;
            Skill.Update();
            Skill.Update();
            Skill.Update();
            Assert.AreEqual(0, timesCalled);
        }
        
        [Test]
        public void TestRemoveWasCalled()
        {
            var timesCalled = 0;
            Skill.SetMaxLevel(1);
            Skill.OnRemoveCallback = () => timesCalled++;
            Skill.Level = 1;
            Skill.Update();
            Skill.Level = 0;
            Skill.Update();
            Assert.AreEqual(1, timesCalled);
        }
    }
}