using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine;
using Hedra.Engine.ItemSystem.WeaponSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Moq;
using NUnit.Framework;
using OpenTK;

namespace HedraTests.Player.Skills
{
    [TestFixture]
    public class WeaponAttackTest : SkillTest<WeaponAttack>
    {

        [Test]
        public void TestPrimaryAttackIsContinous()
        {
            var executedAttackTimes = 0;
            var weaponMock = new Mock<Weapon>(new VertexData());
            weaponMock.Setup(W => W.Attack1(It.IsAny<IHumanoid>()))
                .Callback( () => executedAttackTimes++);
            Player.LeftWeapon = weaponMock.Object;
            Skill.SetType(Player.LeftWeapon, AttackType.Primary);
            Skill.Use();
            Assert.AreEqual(0, executedAttackTimes);
            
            Skill.Update();
            Skill.Update();
            Skill.Update();
            Skill.Update();
            Skill.Update();
            
            Skill.KeyUp();
            Assert.AreEqual(5, executedAttackTimes);
        }
        
        [Test]
        public void TestPrimaryAttackDeactivates()
        {
            var executedAttackTimes = 0;
            var weaponMock = new Mock<Weapon>(new VertexData());
            weaponMock.Setup(W => W.Attack1(It.IsAny<IHumanoid>()))
                .Callback( () => executedAttackTimes++);
            Player.LeftWeapon = weaponMock.Object;
            Skill.SetType(Player.LeftWeapon, AttackType.Primary);
            Skill.Use();
            Assert.AreEqual(0, executedAttackTimes);          
            Skill.Update();     
            Skill.KeyUp();
            
            Skill.Update(); 
            Skill.Update(); 
            Skill.Update();
            Assert.AreEqual(1, executedAttackTimes);
        }

        [Test]
        public void TestAttackCanBeCharged()
        {
            var wasAttackExecuted = false;
            var weaponMock = new Mock<Weapon>(new VertexData());
            weaponMock.Setup(W => W.Attack2(It.IsAny<IHumanoid>(), It.IsAny<AttackOptions>()))
                .Callback( () => wasAttackExecuted = true);
            Player.LeftWeapon = weaponMock.Object;
            Skill.SetType(Player.LeftWeapon, AttackType.Secondary);
            
            Assert.IsFalse(Skill.IsCharging);
            Skill.Use();
            Assert.IsTrue(Skill.IsCharging);
            
            Time.Set(.5f);
            Skill.Update();
            Assert.Greater(Skill.Charge, 0);
            
            Skill.KeyUp();
            Assert.AreEqual(Skill.Charge, 0);
            
            Assert.True(wasAttackExecuted);
        }
        
        [Test]
        public void TestIconsExistAndWork()
        {
            var weapons = WeaponFactory.GetTypes().ToList();
            var defaultIds = this.GetDefaultIconIds();
            var existingIds = new List<uint>();
            weapons.Remove(typeof(Hands));
            for (var i = 0; i < weapons.Count; i++)
            {
                var weapon = (Weapon) Activator.CreateInstance(weapons[i], new VertexData());
                Skill.SetType(weapon, AttackType.Primary);
                var primaryId = Skill.TextureId;
                Skill.SetType(weapon, AttackType.Secondary);
                var secondaryId = Skill.TextureId;
                Assert.False(Array.IndexOf(defaultIds, primaryId) != -1, 
                    $"Weapon '{weapons[i].Name}' has the default primary TextureId");
                Assert.False(Array.IndexOf(defaultIds, secondaryId) != -1, 
                    $"Weapon '{weapons[i].Name}' has the default secondary TextureId");
                Assert.False(existingIds.IndexOf(primaryId) != -1, 
                    $"Weapon '{weapons[i].Name}' has the primary TextureId as the '{weapons[existingIds.IndexOf(primaryId) / 2]}'");
                Assert.False(existingIds.IndexOf(secondaryId) != -1, 
                    $"Weapon '{weapons[i].Name}' has the secondary TextureId as the '{weapons[existingIds.IndexOf(secondaryId) / 2]}'");
                existingIds.Add(primaryId);
                existingIds.Add(secondaryId);
            }
        }

        private uint[] GetDefaultIconIds()
        {
            var hands = new Hands();
            var list = new uint[2];
            Skill.SetType(hands, AttackType.Primary);
            list[0] = Skill.TextureId;
            Skill.SetType(hands, AttackType.Secondary);
            list[1] = Skill.TextureId;
            return list;
        }
    }
}