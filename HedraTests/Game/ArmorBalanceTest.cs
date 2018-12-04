using System;
using System.Collections.Generic;
using Hedra.Engine.Game;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using NUnit.Framework;

namespace HedraTests.Game
{
    [TestFixture]
    public class ArmorBalanceTest : BaseEquipmentTest<ArmorBalanceTest>, IBalanceTestCompliant
    {
        private readonly ArmorBalanceSheet _sheet = new ArmorBalanceSheet();
/*
        [TestCaseSource(typeof(ArmorBalanceTest), nameof(Some), new object[] {EquipmentType.Helmet})]
        public void TestHelmetDefense(Item Armor)
        {
            AssertComplies(Armor, _sheet.HelmetDefense, () => Armor.GetAttribute<float>(CommonAttributes.Defense));
        }

        [TestCaseSource(typeof(ArmorBalanceTest), nameof(Some), new object[] {EquipmentType.Chestplate})]
        public void TestChestplateDamage(Item Armor)
        {
            AssertComplies(Armor, _sheet.ChestplateDefense, () => Armor.GetAttribute<float>(CommonAttributes.Defense));
        }

        [TestCaseSource(typeof(ArmorBalanceTest), nameof(Some), new object[] {EquipmentType.Pants})]
        public void TestPantsDamage(Item Armor)
        {
            AssertComplies(Armor, _sheet.PantsDefense, () => Armor.GetAttribute<float>(CommonAttributes.Defense));
        }

        [TestCaseSource(typeof(ArmorBalanceTest), nameof(Some), new object[] {EquipmentType.Boots})]
        public void TestBootsDamage(Item Armor)
        {
            AssertComplies(Armor, _sheet.BootsDefense, () => Armor.GetAttribute<float>(CommonAttributes.Defense));
        }
*/
        public override bool Complies(Item Equipment)
        {
            return Equipment.IsArmor;
        }
    }
}