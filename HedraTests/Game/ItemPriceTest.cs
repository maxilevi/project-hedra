using System.Collections.Generic;
using System.Runtime.InteropServices;
using Hedra.Engine.Game;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player.Inventory;
using HedraTests.ItemSystem;
using NUnit.Framework;

namespace HedraTests.Game
{
    [TestFixture]
    public class ItemPriceTest
    {
        private readonly TradeManagerMock _trader = new TradeManagerMock();
        [TestCaseSource(nameof(All))]
        public void TestItemPriceIsWithinRange(Item Item)
        {
            AssertComplies(Item);
        }

        private void AssertComplies(Item Item)
        {
            var expectedPrice = CalculatePrice(Item);
            var currentPrice = _trader.ItemPrice(Item);
            Assert.AreEqual(expectedPrice, currentPrice, $"Price should be '{expectedPrice}' but was '{currentPrice}'");
            TestContext.WriteLine($"Price should be '{expectedPrice}' but was '{currentPrice}'");       
        }

        private static float CalculatePrice(Item Item)
        {
            var price = Item.HasAttribute(CommonAttributes.Amount) ? Item.GetAttribute<int>(CommonAttributes.Amount) : 0;
            if (Item.IsEquipment) price += 20;
            price *= (int) Item.Tier;
            return price;
        }
        
        private static IEnumerable<Item> All()
        {
            AssetManager.Provider = new SimpleAssetProvider();
            ItemFactory.LoadModules(GameLoader.AppPath);
            var templates = ItemFactory.Templater.Templates;
            for (var i = 0; i < templates.Length; i++)
            {
                var newItem = Item.FromTemplate(templates[i]);
                yield return newItem;
            }
        }        
    }
}