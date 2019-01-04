using System;
using System.Drawing;
using System.Linq;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Sound;
using Hedra.Sound;

namespace Hedra.Engine.Player.Inventory
{
    public delegate void OnTransactionCompleteEventHandler(Item Item, int Price);

    public class TradeManager
    {
        public event OnTransactionCompleteEventHandler OnTransactionComplete;
        private readonly InventoryArrayInterface _buyerInterface;
        private readonly InventoryArrayInterface _sellerInterface;

        public TradeManager(InventoryArrayInterface BuyerInterface, InventoryArrayInterface SellerInterface)
        {
            _buyerInterface = BuyerInterface;
            _sellerInterface = SellerInterface;
        }

        public int ItemPrice(Item Item)
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
                    return ItemPrice(ItemPool.Grab(Item.GetAttribute<string>(CommonAttributes.Output))) / 2;
                
                price *= (int) (Item.Tier+1);
            }
            else
            {
                price = Item.GetAttribute<int>(CommonAttributes.Price);
            }
            var amount = Item.HasAttribute(CommonAttributes.Amount) ? Item.GetAttribute<int>(CommonAttributes.Amount) : 1;
            //if(amount != int.MaxValue)
            //    price *= amount;
            return (int) (price * GetPriceMultiplier(Item));
        }

        protected virtual float GetPriceMultiplier(Item Item)
        {
            return _buyerInterface.Array.Contains(Item) ? 0.5f : 1.25f;
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
        
        public void ProcessTrade(Humanoid Buyer, Humanoid Seller,
    InventoryArrayInterface BuyerInterface, InventoryArrayInterface SellerInterface, Item Item, int Price)
        {
            if (Buyer.Gold >= Price || Buyer.Gold == int.MaxValue)
            {
                if (Buyer.Gold != int.MaxValue) Buyer.Gold -= Price;
                if (Seller.Gold != int.MaxValue) Seller.Gold += Price;


                if (Item.HasAttribute(CommonAttributes.Amount))
                {
                    var amount = Item.GetAttribute<int>(CommonAttributes.Amount);
                    var isFinite = amount != int.MaxValue;
                    if (isFinite)
                    {
                        if (Item.GetAttribute<int>(CommonAttributes.Amount) > 1)
                            Item.SetAttribute(CommonAttributes.Amount,
                                Item.GetAttribute<int>(CommonAttributes.Amount) - 1);
                        else
                            SellerInterface.Array.RemoveItem(Item);
                    }

                    var oneItem = Item.FromArray(Item.ToArray());
                    oneItem.SetAttribute(CommonAttributes.Amount, 1);
                    if (!BuyerInterface.Array.AddItem(oneItem))
                    {
                        World.DropItem(oneItem, Buyer.Position);
                        SoundPlayer.PlaySound(SoundType.NotificationSound, Buyer.Position);
                    }
                }
                else
                {
                    SellerInterface.Array.RemoveItem(Item);

                    if (!BuyerInterface.Array.AddItem(Item))
                    {
                        World.DropItem(Item, Buyer.Position);
                        SoundPlayer.PlaySound(SoundType.NotificationSound, Buyer.Position);
                    }
                }
                SoundPlayer.PlaySound(SoundType.TransactionSound, Buyer.Position);
            }
            else
            {
                Buyer.MessageDispatcher.ShowNotification(Translations.Get("not_enough_money"), Color.Red, 3f);
            }
            OnTransactionComplete?.Invoke(Item, Price);
        }
    }
}
