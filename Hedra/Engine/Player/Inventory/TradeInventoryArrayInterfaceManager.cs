using System;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Sound;
using OpenTK.Input;

namespace Hedra.Engine.Player.Inventory
{
    public class TradeInventoryArrayInterfaceManager : InventoryArrayInterfaceManager
    {
        private readonly TradeInventoryInterfaceItemInfo _itemInfoInterface;
        private readonly InventoryArrayInterface _playerInterface;
        private readonly InventoryArrayInterface _merchantInterface;

        public TradeInventoryArrayInterfaceManager(TradeInventoryInterfaceItemInfo ItemInfoInterface,
            InventoryArrayInterface PlayerInterface, InventoryArrayInterface MerchantInterface) 
            : base(ItemInfoInterface, PlayerInterface, MerchantInterface)
        {
            _itemInfoInterface = ItemInfoInterface;
            _playerInterface = PlayerInterface;
            _merchantInterface = MerchantInterface;
        }

        protected override void Use(object Sender, MouseButtonEventArgs EventArgs)
        {
            if (EventArgs.Button != MouseButton.Right) return;
            var button = (Button)Sender;
            var itemIndex = this.IndexByButton(button);
            var arrayInterface = this.InterfaceByButton(button);
            var item = arrayInterface.Array[itemIndex];
            arrayInterface.Array[itemIndex] = null;

            if (arrayInterface == _playerInterface)
            {
                _merchantInterface.Array.AddItem(item);
                this.ProcessSell(item);
            }
            else
            {
                _playerInterface.Array.AddItem(item);
                this.ProcessBuy(item);
            }
            this.UpdateView();
            SoundManager.PlaySoundInPlayersLocation(SoundType.ButtonClick);
        }

        private void ProcessBuy(Item Item)
        {
            var price = ItemPrice(Item) * 1.0f;
            SoundManager.PlaySoundInPlayersLocation(SoundType.TransactionSound);
        }

        private void ProcessSell(Item Item)
        {
            var price = ItemPrice(Item) * .75f;
            SoundManager.PlaySoundInPlayersLocation(SoundType.TransactionSound);
        }

        public static int ItemPrice(Item Item)
        {
            float price = 0;
            var attributes = Item.GetAttributes();
            for (var i = 0; i < attributes.Length; i++)
            {
                if (attributes[i].Name == CommonAttributes.Amount.ToString())
                {
                    price += Item.GetAttribute<int>(CommonAttributes.Amount);
                    continue;
                }
                var typeList = new[] { typeof(float), typeof(double), typeof(long) };
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
            price *= (int) Item.Tier+1;

            return (int)price;
        }

    }
}
