using System;
using Hedra.Engine.ItemSystem.WeaponSystem;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Rendering;
using Moq;
using NUnit.Framework;

namespace HedraTests.Player.Skills
{
    [TestFixture]
    public class SpecialAttackSkillTest : SkillTest<SpecialAttackSkillTest.SpecialAttackSkillMock>
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            Player.Mana = 10000;
        }
        
        [Test]
        public void TestSkillCantBeUsedWithoutTheCorrectWeapon()
        {
            Player.LeftWeapon = new Claw(new VertexData());
            Skill.Callback = W => Assert.Fail($"Skill was used, even though the required weapon is {typeof(Bow)} and the given is {W.GetType()}");
            Skill.Level = 1;
            Assert.False(Skill.MeetsRequirements());
        }
        
        [Test]
        public void TestSkillIsGrayscaleWhenIncorretWeapon()
        {
            Player.LeftWeapon = new Claw(new VertexData());
            Assert.True(Skill.HasGrayscale);
        }
        
        [Test]
        public void TestSkillCanBeUsedWithCorrectWeapon()
        {
            var wasCalled = false;
            Player.LeftWeapon = new Bow(new VertexData());
            Skill.Callback = W => wasCalled = true;
            Skill.Level = 1;
            Assert.True(Skill.MeetsRequirements());
            Skill.Use();
            Assert.True(wasCalled);
        }
        
        [Test]
        public void TestSkillHasNotGrayscaleWithTheCorrectWeapon()
        {
            Player.LeftWeapon = new Bow(new VertexData());
            Assert.False(Skill.HasGrayscale);
        }
        
        public class SpecialAttackSkillMock : SpecialAttackSkill<Bow>
        {
            public bool HasGrayscale => Grayscale;
            public override string Description => string.Empty;
            public Action<Bow> Callback { get; set; }
        
            protected override void BeforeUse(Bow Weapon)
            {
                Callback?.Invoke(Weapon);
            }
        }
    }
}