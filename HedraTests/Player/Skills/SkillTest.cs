using Hedra.Engine;
using Hedra.Engine.ItemSystem.WeaponSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Moq;
using NUnit.Framework;

namespace HedraTests.Player.Skills
{
    public abstract class SkillTest :  BaseTest
    {
        protected PlayerMock Player;
        
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            Player = new PlayerMock();
            Player = new PlayerMock();
            var weaponMock = new Mock<Weapon>(new VertexData());
            weaponMock.Setup(W => W.Attack1(It.IsAny<IHumanoid>()));
            Player.LeftWeapon = weaponMock.Object;
            GameManager.Player = Player;
        }
    }
}