using Hedra.Engine.Generation;
using Hedra.Engine.Rendering;
using Hedra.Engine.SkillSystem.Archer;
using Hedra.Rendering;
using Hedra.WeaponSystem;
using NUnit.Framework;
using System.Numerics;

namespace HedraTests.Player.Skills.Archer
{
    //[TestFixture]
    public class PunctureTest : SkillTest<Puncture>
    {
        private Bow _bow;
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _bow = new Bow(new VertexData());
            Player.LeftWeapon = _bow;
        }
        /*
        [Test]
        public void TestPunctureIsApplied()
        {
            Skill.Use();
            var invocationList = _bow.BowModifiers.GetInvocationList();
            var proj = _bow.ShootArrow(Player, Vector3.Zero, Vector3.Zero, AttackOptions.Default);
            for (var i = 0; i < invocationList.Length; i++)
            {
                var inv = invocationList[i];
                inv.Method.Invoke(_bow, new object[]{ proj });
            }
        }

        [Test]
        public void TestPunctureIsRemoved()
        {

        }*/
    }
}