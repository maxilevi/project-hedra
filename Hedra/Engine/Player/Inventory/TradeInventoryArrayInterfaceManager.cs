using Hedra.Engine.Windowing;
using Hedra.Sound;
using Silk.NET.Input;
using Button = Hedra.Engine.Rendering.UI.Button;

namespace Hedra.Engine.Player.Inventory
{
    public class TradeInventoryArrayInterfaceManager : InventoryArrayInterfaceManager
    {
        private readonly InventoryArrayInterface _buyerInterface;
        private readonly TradeInventoryInterfaceItemInfo _itemInfoInterface;
        private readonly InventoryArrayInterface _sellerInterface;
        private Humanoid _buyer;
        private TradeManager _manager;
        private Humanoid _seller;

        public TradeInventoryArrayInterfaceManager(TradeInventoryInterfaceItemInfo ItemInfoInterface,
            InventoryArrayInterface BuyerInterface, InventoryArrayInterface SellerInterface)
            : base(ItemInfoInterface, BuyerInterface, SellerInterface)
        {
            _itemInfoInterface = ItemInfoInterface;
            _buyerInterface = BuyerInterface;
            _sellerInterface = SellerInterface;
        }

        public event OnTransactionCompleteEventHandler OnTransactionComplete;

        public void SetTraders(Humanoid Buyer, Humanoid Seller)
        {
            _buyer = Buyer;
            _seller = Seller;
            _manager = _buyer == null && Seller == null ? null : new TradeManager(_buyerInterface, _sellerInterface);
            _itemInfoInterface.SetManager(_manager);
        }

        protected override void Interact(object Sender, MouseButtonEventArgs EventArgs)
        {
        }

        protected override void Use(object Sender, MouseButtonEventArgs EventArgs)
        {
            if (EventArgs.Button != MouseButton.Right) return;
            var button = (Button)Sender;
            var itemIndex = IndexByButton(button);
            var arrayInterface = InterfaceByButton(button);
            var item = arrayInterface.Array[itemIndex];
            if (item == null || item.IsGold) return;

            var price = _manager.ItemPrice(item);
            var isBuying = arrayInterface != _buyerInterface;
            if (isBuying)
                _manager.ProcessTrade(_buyer, _seller, _buyerInterface, _sellerInterface, item, price);
            else
                _manager.ProcessTrade(_seller, _buyer, _sellerInterface, _buyerInterface, item, price);
            OnTransactionComplete?.Invoke(item, price, isBuying ? TransactionType.Buy : TransactionType.Sell);
            UpdateView();
            SoundPlayer.PlayUISound(SoundType.ButtonClick);
        }
    }
}