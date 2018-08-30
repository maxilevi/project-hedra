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
    public class PassiveSkillTest : SkillTest
    {
        private PassiveSkillMock _skill;
        
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _skill = new PassiveSkillMock();
            _skill.Initialize(Vector2.Zero, Vector2.One, new Panel(), Player);
        }

        [Test]
        public void TestSkillDoesntSurpassMaxLevel()
        {
            var abilityTreeMock = new Mock<IAbilityTree>();
            abilityTreeMock.Setup(A => A.SetPoints(It.IsAny<Type>(), It.IsAny<int>())).Callback(delegate(Type T, int L)
            {
                 _skill.Level = L;
            });
            abilityTreeMock.Setup(A => A.SetPoints(It.IsAny<int>(), It.IsAny<int>())).Callback(delegate(int I, int L)
            {
                _skill.Level = L;
            });
            Player.AbilityTree = abilityTreeMock.Object;
            _skill.Level = 0;
            _skill.SetMaxLevel(3);
            _skill.Level++;
            _skill.Update();
            _skill.Level++;
            _skill.Update();
            _skill.Level++;
            _skill.Update();
            _skill.Level++;
            _skill.Update();
            _skill.Level++;
            _skill.Update();
            Assert.AreEqual(3, _skill.Level);
        }
        
        [Test]
        public void TestOnChangeWasCalled()
        {
            var timesCalled = 0;
            _skill.OnChangeCallback = () => timesCalled++;
            _skill.Update();
            _skill.Update();
            _skill.Update();
            Assert.AreEqual(3, timesCalled);
        }
    }
}