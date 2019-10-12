using System;
using Hedra.Engine;
using Hedra.Engine.Game;
using Hedra.Engine.Player;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.SkillSystem;
using Hedra.Game;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using Hedra.WeaponSystem;
using Moq;
using NUnit.Framework;
using OpenToolkit.Mathematics;

namespace HedraTests.Player.Skills
{
    public abstract class SkillTest<T> : BaseTest where T : AbstractBaseSkill, new()
    {
        protected PlayerMock Player;
        protected T Skill;
        
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            Player = new PlayerMock();
            var weaponMock = new Mock<Weapon>(new VertexData());
            Player.LeftWeapon = weaponMock.Object;
            GameManager.Player = Player;
            Skill = new T();
            Skill.InitializeUI(Vector2.Zero, Vector2.One, new Panel());
            Skill.Initialize(Player);
            var abilityTreeMock = new Mock<IAbilityTree>();
            abilityTreeMock.Setup(A => A.SetPoints(It.IsAny<Type>(), It.IsAny<int>())).Callback(delegate(Type T, int L)
            {
                Skill.Level = L;
            });
            abilityTreeMock.Setup(A => A.SetPoints(It.IsAny<int>(), It.IsAny<int>())).Callback(delegate(int I, int L)
            {
                Skill.Level = L;
            });
            Player.AbilityTree = abilityTreeMock.Object;
        }
    }
}