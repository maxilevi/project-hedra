using Hedra.Engine.ItemSystem;
using Hedra.Items;
using Hedra.Localization;
using Hedra.Sound;
using SixLabors.ImageSharp;

namespace Hedra.Engine.Player.Inventory
{
    public delegate void OnTransactionCompleteEventHandler(Item Item, int Price, TransactionType Type);

    public class TradeManager
    {
        private static readonly StaticTradeManager _staticTrader;

        private readonly InventoryArrayInterface _buyerInterface;
        private readonly InventoryArrayInterface _sellerInterface;

        static TradeManager()
        {
            _staticTrader = new StaticTradeManager();
        }

        public TradeManager(InventoryArrayInterface BuyerInterface, InventoryArrayInterface SellerInterface)
        {
            _buyerInterface = BuyerInterface;
            _sellerInterface = SellerInterface;
        }

        public int ItemPrice(Item Item)
        {
            /*
            var amount = Item.HasAttribute(CommonAttributes.Amount) ? Item.GetAttribute<int>(CommonAttributes.Amount) : 1;
            if(amount != int.MaxValue)
                price *= amount;*/
            return (int)(Trader.SingleItemPrice(Item) * GetPriceMultiplier(Item));
        }

        protected virtual float GetPriceMultiplier(Item Item)
        {
            return _buyerInterface.Array.Contains(Item) ? Trader.SellMultiplier : Trader.BuyMultiplier;
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
        }

        public static int Price(Item Item)
        {
            return _staticTrader.ItemPrice(Item);
        }

        private class StaticTradeManager : TradeManager
        {
            public StaticTradeManager() : base(null, null)
            {
            }

            protected override float GetPriceMultiplier(Item Item)
            {
                return 1;
            }
        }
    }

    public enum TransactionType
    {
        Buy,
        Sell
    }
}