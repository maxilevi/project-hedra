using System.Drawing;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.UI;
using Hedra.Items;
using OpenTK;

namespace Hedra.Engine.Player.Inventory
{
    public class TradeInventoryInterfaceItemInfo : InventoryInterfaceItemInfo
    {
        private TradeManager _manager;

        public TradeInventoryInterfaceItemInfo(InventoryItemRenderer Renderer) : base(Renderer)
        {
            HintText.TextFont = FontCache.GetBold(10);
            HintText.TextColor = Color.Gold;
        }

        protected override void AddHint()
        {
            var priceString = _manager.ItemPrice(CurrentItem).ToString();
            if (CurrentItem.HasAttribute(CommonAttributes.Amount)
                && CurrentItem.GetAttribute<int>(CommonAttributes.Amount) == int.MaxValue)
            {
                var clone = Item.FromArray(CurrentItem.ToArray());
                clone.SetAttribute(CommonAttributes.Amount, 1);
                priceString = _manager.ItemPrice(clone).ToString();
            }
            HintText.Text = $"{priceString} G";
            HintText.Enable();
            HintTexture.Enable();
        }

        public override void Show(Item Item)
        {
            if (Item == null || Item.IsGold) return;
            base.Show(Item);
        }

        public void SetManager(TradeManager Manager)
        {
            _manager = Manager;
        }
    }
}
