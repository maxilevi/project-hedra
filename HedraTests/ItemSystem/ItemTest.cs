using System;
using System.Collections.Generic;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ItemSystem.Templates;
using Hedra.Engine.Management;
using Hedra.Items;
using NUnit.Framework;

namespace HedraTests.ItemSystem
{
    [TestFixture]
    public class ItemTest : BaseTest
    {
        private Random _rng;
        public ItemTest()
        {
            _rng = new Random();
        }
        
        [TestCaseSource(nameof(All))]
        public void TestItemSerializationIsEqual(ItemTemplate Template)
        {
            var item = ItemPool.Grab(Template.Name, Unique.RandomSeed());
            if(item.HasAttribute(CommonAttributes.Amount))
                item.SetAttribute(CommonAttributes.Amount, _rng.Next(0, int.MaxValue));
            var newItem = Item.FromArray(item.ToArray());
            
            AssertAreSame(item, newItem);
        }

        public static void AssertAreSame(Item A, Item B)
        {
            Assert.AreEqual(A.Name, B.Name);
            Assert.AreEqual(A.Description, B.Description);
            Assert.AreEqual(A.Tier, B.Tier);
            Assert.AreEqual(A.DisplayName, B.DisplayName);
            Assert.AreEqual(A.EquipmentType, B.EquipmentType);

            var attributesA = A.GetAttributes();
            var attributesB = B.GetAttributes();
            Assert.AreEqual(attributesA.Length, attributesB.Length);
            for (var i = 0; i < attributesA.Length; i++)
            {
                Assert.AreEqual(attributesA[i].Name, attributesB[i].Name);
                Assert.AreEqual(attributesA[i].Value, attributesB[i].Value,
                    $"Expected '{attributesA[i].Name}' to have {attributesA[i].Value} but was {attributesB[i].Value}");
            }
        }
        
        private static IEnumerable<ItemTemplate> All()
        {
            AssetManager.Provider = new DummyAssetProvider();
            ItemLoader.LoadModules(GameLoader.AppPath);
            var templates = ItemLoader.Templater.Templates;
            for (var i = 0; i < templates.Length; i++)
            {
                yield return templates[i];
            }
        } 
    }
}