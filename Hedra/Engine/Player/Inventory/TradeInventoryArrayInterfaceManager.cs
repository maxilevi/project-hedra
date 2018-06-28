using System;
using System.Drawing;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Sound;
using OpenTK.Input;

namespace Hedra.Engine.Player.Inventory
{
    internal class TradeInventoryArrayInterfaceManager : InventoryArrayInterfaceManager
    {
        public event OnTransactionCompleteEventHandler OnTransactionComplete;
        private readonly TradeInventoryInterfaceItemInfo _itemInfoInterface;
        private readonly InventoryArrayInterface _buyerInterface;
        private readonly InventoryArrayInterface _sellerInterface;
        private TradeManager _manager;
        private Humanoid _buyer;
        private Humanoid _seller;

        public TradeInventoryArrayInterfaceManager(TradeInventoryInterfaceItemInfo ItemInfoInterface,
            InventoryArrayInterface BuyerInterface, InventoryArrayInterface SellerInterface) 
            : base(ItemInfoInterface, BuyerInterface, SellerInterface)
        {
            _itemInfoInterface = ItemInfoInterface;
            _buyerInterface = BuyerInterface;
            _sellerInterface = SellerInterface;
        }

        public void SetTraders(Humanoid Buyer, Humanoid Seller)
        {
            _buyer = Buyer;
            _seller = Seller;
            _manager = _buyer == null && Seller == null ?  null : new TradeManager(_buyerInterface, _sellerInterface);
            if(_manager != null) _manager.OnTransactionComplete += (I, P) => OnTransactionComplete?.Invoke(I, P);
            _itemInfoInterface.SetManager(_manager);
        }

        protected override void Interact(object Sender, MouseButtonEventArgs EventArgs) {}

        protected override void Use(object Sender, MouseButtonEventArgs EventArgs)
        {
            if (EventArgs.Button != MouseButton.Right) return;
            var button = (Button)Sender;
            var itemIndex = this.IndexByButton(button);
            var arrayInterface = this.InterfaceByButton(button);
            var item = arrayInterface.Array[itemIndex];
            if (item == null || item.IsGold) return;

            var price = _manager.ItemPrice(item);
            if (arrayInterface != _buyerInterface)
            {
                _manager.ProcessTrade(_buyer, _seller, _buyerInterface, _sellerInterface, item, price);
            }
            else
            {
                _manager.ProcessTrade(_seller, _buyer, _sellerInterface, _buyerInterface, item, price);
            }
            this.UpdateView();
            SoundManager.PlayUISound(SoundType.ButtonClick);
        }
    }
}
