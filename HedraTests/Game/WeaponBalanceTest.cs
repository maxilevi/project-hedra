using System;
using System.Collections.Generic;
using Hedra.Engine.Game;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Items;
using NUnit.Framework;

namespace HedraTests.Game
{
    [TestFixture]
    public class WeaponBalanceTest : BaseEquipmentTest<WeaponBalanceTest>, IBalanceTestCompliant
    {
        private readonly WeaponBalanceSheet _sheet = new WeaponBalanceSheet();
        
        [TestCaseSource(typeof(WeaponBalanceTest), nameof(Some), new object[] { EquipmentType.Bow })]
        public void TestBowDamage(Item Weapon)
        {
            AssertComplies(Weapon, _sheet.BowDamage, () => Weapon.GetAttribute<float>(CommonAttributes.Damage));
        }
        
        [TestCaseSource(typeof(WeaponBalanceTest), nameof(Some), new object[] { EquipmentType.Sword })]
        public void TestSwordDamage(Item Weapon)
        {
            AssertComplies(Weapon, _sheet.SwordDamage, () => Weapon.GetAttribute<float>(CommonAttributes.Damage));
        }
        
        [TestCaseSource(typeof(WeaponBalanceTest), nameof(Some), new object[] { EquipmentType.Hammer })]
        public void TestHammerDamage(Item Weapon)
        {
            AssertComplies(Weapon, _sheet.HammerDamage, () => Weapon.GetAttribute<float>(CommonAttributes.Damage));
        }

        [TestCaseSource(typeof(WeaponBalanceTest), nameof(Some), new object[] { EquipmentType.Axe })]
        public void TestAxeDamage(Item Weapon)
        {
            AssertComplies(Weapon, _sheet.AxeDamage, () => Weapon.GetAttribute<float>(CommonAttributes.Damage));
        }
        
        [TestCaseSource(typeof(WeaponBalanceTest), nameof(Some), new object[] { EquipmentType.Knife })]
        public void TestKnifeDamage(Item Weapon)
        {
            AssertComplies(Weapon, _sheet.KnifeDamage, () => Weapon.GetAttribute<float>(CommonAttributes.Damage));
        }
        
        [TestCaseSource(typeof(WeaponBalanceTest), nameof(Some), new object[] { EquipmentType.Claw })]
        public void TestClawDamage(Item Weapon)
        {
            AssertComplies(Weapon, _sheet.ClawDamage, () => Weapon.GetAttribute<float>(CommonAttributes.Damage));
        }
        
        [TestCaseSource(typeof(WeaponBalanceTest), nameof(Some), new object[] { EquipmentType.Katar })]
        public void TestKatarDamage(Item Weapon)
        {
            AssertComplies(Weapon, _sheet.KatarDamage, () => Weapon.GetAttribute<float>(CommonAttributes.Damage));
        }
        
        [TestCaseSource(typeof(WeaponBalanceTest), nameof(Some), new object[] { EquipmentType.DoubleBlades })]
        public void TestBladesDamage(Item Weapon)
        {
            AssertComplies(Weapon, _sheet.BladesDamage, () => Weapon.GetAttribute<float>(CommonAttributes.Damage));
        }
              
        [TestCaseSource(typeof(WeaponBalanceTest), nameof(Some), new object[] { EquipmentType.Staff })]
        public void TestStaffDamage(Item Weapon)
        {
            AssertComplies(Weapon, _sheet.StaffDamage, () => Weapon.GetAttribute<float>(CommonAttributes.Damage));
        }     
        
        [TestCaseSource(nameof(All))]
        public void TestWeaponsAttackSpeed(Item Weapon)
        {
            AssertComplies(Weapon, _sheet.ItemAttackSpeed, () => Weapon.GetAttribute<float>(CommonAttributes.AttackSpeed));
        }
        
        [TestCaseSource(typeof(WeaponBalanceTest), nameof(Some), new object[] { EquipmentType.Bow })]
        public void TestBowAttackSpeed(Item Weapon)
        {
            AssertComplies(Weapon, _sheet.BowAttackSpeed, () => Weapon.GetAttribute<float>(CommonAttributes.AttackSpeed));
        }
        
        public bool Complies(Item Equipment)
        {
            return Equipment.IsWeapon;
        }
    }
}