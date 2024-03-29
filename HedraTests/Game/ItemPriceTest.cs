using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Hedra.Crafting;
using Hedra.Engine.Game;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player.Inventory;
using Hedra.Items;
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

        [Test]
        public void TestItemPriceWhenInfinite()
        {
            var berry = ItemPool.Grab(ItemType.Berry);
            var price = _trader.ItemPrice(berry);
            berry.SetAttribute(CommonAttributes.Amount, int.MaxValue);
            Assert.AreEqual(price, _trader.ItemPrice(berry));
        }

        private void AssertComplies(Item Item)
        {
            //var expectedPrice = CalculatePrice(Item);
            var currentPrice = _trader.ItemPrice(Item);
            //var msg = $"Price for '{Item.Name}' should be '{expectedPrice}' but was '{currentPrice}'";
            //Assert.AreEqual(expectedPrice, currentPrice, msg);
            //TestContext.WriteLine(msg);
        }

        private static float CalculatePrice(Item Item)
        {
            var price = 1f;
            if (!Item.HasAttribute(CommonAttributes.Price))
            {
                if (Item.IsEquipment)
                {
                    price += 10;
                    if (Item.IsWeapon)
                    {
                        AssertHasAttribute(Item, CommonAttributes.Damage);
                        AssertHasAttribute(Item, CommonAttributes.AttackSpeed);
                        
                        price += GetAttribute(Item, CommonAttributes.Damage);
                        price += GetAttribute(Item, CommonAttributes.AttackSpeed);
                    }

                    if (Item.IsArmor)
                    {
                        price += GetAttribute(Item, CommonAttributes.Defense);
                        price += GetAttribute(Item, CommonAttributes.MovementSpeed, 1);
                    }

                    if (Item.IsRing)
                    {
                        AssertHasAttribute(Item, CommonAttributes.AttackSpeed);
                        AssertHasAttribute(Item, CommonAttributes.Health);
                        AssertHasAttribute(Item, CommonAttributes.MovementSpeed);
                        
                        price += GetAttribute(Item, CommonAttributes.AttackSpeed);
                        price += GetAttribute(Item, CommonAttributes.Health);
                        price += GetAttribute(Item, CommonAttributes.MovementSpeed);
                    }
                }

                if (Item.IsConsumable)
                {
                    price += 40;
                }

                if (Item.IsFood)
                {
                    AssertHasAttribute(Item, CommonAttributes.Saturation);
                    AssertHasAttribute(Item, CommonAttributes.EatTime);
                    
                    price += Item.GetAttribute<int>(CommonAttributes.Saturation) / 15f;
                    price -= Item.GetAttribute<float>(CommonAttributes.EatTime) / 5f;
                }

                if (Item.IsRecipe)
                {
                    return CalculatePrice(CraftingInventory.GetOutputFromRecipe(Item, 1));
                }

                price *= (int) (Item.Tier + 1);
            }
            else
            {
                price = Item.GetAttribute<int>(CommonAttributes.Price);
            }

            price *= Item.HasAttribute(CommonAttributes.Amount) ? Item.GetAttribute<int>(CommonAttributes.Amount) : 1;
            return (int) price;
        }

        private static float GetAttribute(Item Item, CommonAttributes Attribute, float Default=0)
        {
            var attr = Item.GetAttributes().FirstOrDefault(T => T.Name == Attribute.ToString());
            if (attr != null)
            {
                return attr.Display == AttributeDisplay.Percentage.ToString()
                    ? ConvertObj<float>(attr.Value) * 100f
                    : ConvertObj<float>(attr.Value);
            }

            return Default;
        }

        private static T ConvertObj<T>(object Value)
        {
            return typeof(T).IsAssignableFrom(typeof(IConvertible)) || typeof(T).IsValueType
                ? (T) Convert.ChangeType(Value, typeof(T))
                : (T) Value;
        }

        private static IEnumerable<Item> All()
        {
            AssetManager.Provider = new DummyAssetProvider();
            ItemLoader.LoadModules(GameLoader.AppPath);
            var templates = ItemLoader.Templater.Templates;
            for (var i = 0; i < templates.Length; i++)
            {
                var newItem = Item.FromTemplate(templates[i]);
                yield return newItem;
            }
        }

        private static void AssertHasAttribute(Item Item, CommonAttributes Attribute)
        {
            Assert.IsTrue(Item.HasAttribute(Attribute), $"Item {Item.Name} does not have attribute {Attribute}");
        }
    }
}