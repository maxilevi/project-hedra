using System;
using System.Linq;
using Hedra.Crafting;
using Hedra.Engine.ItemSystem;

namespace Hedra.Items
{
    public static class Trader
    {
        public const float SellMultiplier = 0.5f;
        public const float BuyMultiplier = 1.25f;
        public static float SingleItemPrice(Item Item)
        {
            if (Item == null) return 0;
            var price = 1f;
            if (!Item.HasAttribute(CommonAttributes.Price))
            {
                if (Item.IsEquipment)
                {
                    price += 10;
                    if (Item.IsWeapon)
                    {
                        price += GetNormalizedAttributeValue(Item, CommonAttributes.Damage);
                        price += GetNormalizedAttributeValue(Item, CommonAttributes.AttackSpeed);
                    }

                    if (Item.IsArmor)
                    {
                        price += GetNormalizedAttributeValue(Item, CommonAttributes.Defense);
                        price += GetNormalizedAttributeValue(Item, CommonAttributes.MovementSpeed);
                    }

                    if (Item.IsRing)
                    {
                        price += GetNormalizedAttributeValue(Item, CommonAttributes.AttackSpeed);
                        price += GetNormalizedAttributeValue(Item, CommonAttributes.Health);
                        price += GetNormalizedAttributeValue(Item, CommonAttributes.MovementSpeed);
                    }
                }

                if (Item.IsConsumable)
                    price += 40;

                if (Item.IsFood)
                {
                    price += Item.GetAttribute<int>(CommonAttributes.Saturation) / 15f;
                    price -= Item.GetAttribute<float>(CommonAttributes.EatTime) / 5f;
                }

                if (Item.IsRecipe)
                {
                    var output = CraftingInventory.GetOutputFromRecipe(Item, 1);
                    return SingleItemPrice(output) * (output.HasAttribute(CommonAttributes.Amount) ? output.GetAttribute<int>(CommonAttributes.Amount) : 1);
                }

                price *= (int) (Item.Tier + 1);
            }
            else
            {
                price = Item.GetAttribute<int>(CommonAttributes.Price);
            }
            return price;
        }
        
        private static float GetNormalizedAttributeValue(Item Item, CommonAttributes Attribute)
        {
            var attr = Item.GetAttributes().First(T => T.Name == Attribute.ToString());
            return attr.Display == AttributeDisplay.Percentage.ToString() 
                ? ConvertObj<float>(attr.Value) * 100f 
                : ConvertObj<float>(attr.Value);
        }
        
        private static T ConvertObj<T>(object Value)
        {
            return typeof(T).IsAssignableFrom(typeof(IConvertible)) || typeof(T).IsValueType
                ? (T) Convert.ChangeType(Value, typeof(T)) 
                : (T) Value;
        }
    }
}