using System;
using System.Collections.Generic;
using Hedra.Engine.Game;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using NUnit.Framework;

namespace HedraTests.Game
{
    [TestFixture]
    public class ItemBalanceTest
    {
        private readonly ItemBalanceSheet _sheet = new ItemBalanceSheet();
        
        [TestCaseSource(typeof(ItemBalanceTest), nameof(SomeWeapons), new object[] { EquipmentType.Bow })]
        public void TestBowDamage(Item Weapon)
        {
            AssertComplies(Weapon, _sheet.BowDamage, () => Weapon.GetAttribute<float>(CommonAttributes.Damage));
        }
        
        [TestCaseSource(typeof(ItemBalanceTest), nameof(SomeWeapons), new object[] { EquipmentType.Sword })]
        public void TestSwordDamage(Item Weapon)
        {
            AssertComplies(Weapon, _sheet.SwordDamage, () => Weapon.GetAttribute<float>(CommonAttributes.Damage));
        }
        
        [TestCaseSource(typeof(ItemBalanceTest), nameof(SomeWeapons), new object[] { EquipmentType.Hammer })]
        public void TestHammerDamage(Item Weapon)
        {
            AssertComplies(Weapon, _sheet.HammerDamage, () => Weapon.GetAttribute<float>(CommonAttributes.Damage));
        }

        [TestCaseSource(typeof(ItemBalanceTest), nameof(SomeWeapons), new object[] { EquipmentType.Axe })]
        public void TestAxeDamage(Item Weapon)
        {
            AssertComplies(Weapon, _sheet.AxeDamage, () => Weapon.GetAttribute<float>(CommonAttributes.Damage));
        }
        
        [TestCaseSource(typeof(ItemBalanceTest), nameof(SomeWeapons), new object[] { EquipmentType.Knife })]
        public void TestKnifeDamage(Item Weapon)
        {
            AssertComplies(Weapon, _sheet.KnifeDamage, () => Weapon.GetAttribute<float>(CommonAttributes.Damage));
        }
        
        [TestCaseSource(typeof(ItemBalanceTest), nameof(SomeWeapons), new object[] { EquipmentType.Claw })]
        public void TestClawDamage(Item Weapon)
        {
            AssertComplies(Weapon, _sheet.ClawDamage, () => Weapon.GetAttribute<float>(CommonAttributes.Damage));
        }
        
        [TestCaseSource(typeof(ItemBalanceTest), nameof(SomeWeapons), new object[] { EquipmentType.Katar })]
        public void TestKatarDamage(Item Weapon)
        {
            AssertComplies(Weapon, _sheet.KatarDamage, () => Weapon.GetAttribute<float>(CommonAttributes.Damage));
        }
        
        [TestCaseSource(typeof(ItemBalanceTest), nameof(SomeWeapons), new object[] { EquipmentType.DoubleBlades })]
        public void TestBladesDamage(Item Weapon)
        {
            AssertComplies(Weapon, _sheet.BladesDamage, () => Weapon.GetAttribute<float>(CommonAttributes.Damage));
        }
        /*
        [TestCaseSource(typeof(ItemBalanceTest), nameof(SomeWeapons), new object[] { EquipmentType.Staff })]
        public void TestStaffDamage(Item Weapon)
        {
            AssertComplies(Weapon, _sheet.StaffDamage, () => Weapon.GetAttribute<float>(CommonAttributes.Damage));
        }
        */
        private static void AssertComplies(Item Item, UniqueBalanceEntry Entry, Func<float> Lambda)
        {
            var multiplier = Entry.ScaleWithLevel ? ((int) Item.Tier) * .35f + 1 : 1;
            var val = Lambda();
            var max = Entry.Max * multiplier;
            var min = Entry.Min * multiplier;
            var msg =
                $"{Item.Name} with rarity '{Item.Tier}' should have {min} < {val} < {max}";
            Assert.GreaterOrEqual(max, val, msg);
            Assert.LessOrEqual(min, val, msg);
            Assert.Pass(msg);
        }

        /* Called via reflection by NUnit */
        private static IEnumerable<Item> SomeWeapons(EquipmentType Match)
        {
            var collection = AllWeapons();
            foreach (var weapon in collection)
            {
                if(weapon.EquipmentType != Match.ToString()) continue;
                yield return weapon;
            }
        }       
        
        /* Called via reflection by NUnit */
        private static IEnumerable<Item> AllWeapons()
        {
            AssetManager.Provider = new SimpleAssetProvider();
            ItemFactory.LoadModules(GameLoader.AppPath);
            var templates = ItemFactory.Templater.Templates;
            for (var i = 0; i < templates.Length; i++)
            {
                var newItem = Item.FromTemplate(templates[i]);
                if (!newItem.IsWeapon) continue;
                yield return newItem;
            }
        }
    }
}