using System.Drawing;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Sound;

namespace Hedra.Engine.Player.Inventory
{
    internal delegate void OnTransactionCompleteEventHandler(Item Item, int Price);

    internal class TradeManager
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
            float price = 1;
            var hasPriceAttribute = Item.HasAttribute(CommonAttributes.Price);
            if (!hasPriceAttribute)
            {
                var attributes = Item.GetAttributes();
                for (var i = 0; i < attributes.Length; i++)
                {
                    if (attributes[i].Name == CommonAttributes.Amount.ToString())
                    {
                        price += 1;
                        continue;
                    }
                    if (attributes[i].Display == AttributeDisplay.Percentage.ToString())
                    {
                        price += (float) attributes[i].Value * 100f;
                        continue;
                    }
                    if (attributes[i].Display == AttributeDisplay.Flat.ToString())
                    {
                        price += (float) attributes[i].Value;
                        continue;
                    }
                    var typeList = new[] {typeof(float), typeof(double), typeof(long)};
                    object selectedType = null;
                    for (var j = 0; j < typeList.Length; j++)
                    {
                        if (typeList[j].IsInstanceOfType(attributes[i].Value))
                        {
                            selectedType = typeList[j];
                            break;
                        }
                    }

                    if (selectedType != null)
                    {
                        var value = Item.GetAttribute<float>(attributes[i].Name);
                        price += Item.EquipmentType != null ? value : value * .025f;
                    }
                }
                price *= Item.EquipmentType != null ? 1.5f : 1.0f;
                price *= (int) Item.Tier + 1;
            }
            else
            {
                price = Item.GetAttribute<int>(CommonAttributes.Price);
            }
            price *= 1 + ((float) Item.Tier / (float) (ItemTier.Misc - 1)) * .5f;
            return (int) (price *(_buyerInterface.Array.Contains(Item) ? 0.5f : 1.25f));
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
                        SoundManager.PlaySound(SoundType.NotificationSound, Buyer.Position);
                    }
                }
                else
                {
                    SellerInterface.Array.RemoveItem(Item);

                    if (!BuyerInterface.Array.AddItem(Item))
                    {
                        World.DropItem(Item, Buyer.Position);
                        SoundManager.PlaySound(SoundType.NotificationSound, Buyer.Position);
                    }
                }
                SoundManager.PlaySound(SoundType.TransactionSound, Buyer.Position);
            }
            else
            {
                Buyer.MessageDispatcher.ShowNotification("NOT ENOUGH MONEY", Color.Red, 3f);
            }
            OnTransactionComplete?.Invoke(Item, Price);
        }
    }
}
