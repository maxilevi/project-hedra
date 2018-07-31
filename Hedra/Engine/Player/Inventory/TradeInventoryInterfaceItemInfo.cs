using System.Drawing;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Player.Inventory
{
    public class TradeInventoryInterfaceItemInfo : InventoryInterfaceItemInfo
    {
        private readonly Texture _backgroundTexture;
        private readonly GUIText _priceText;
        private readonly Panel _panel;
        private TradeManager _manager;

        public TradeInventoryInterfaceItemInfo(InventoryItemRenderer Renderer) : base(Renderer)
        {
            _panel = new Panel();
            _backgroundTexture = new Texture("Assets/UI/InventoryBackground.png", Vector2.UnitY * -.35f, Vector2.One * .15f);
            _priceText = new GUIText(string.Empty, _backgroundTexture.Position, Color.Gold, FontCache.Get(AssetManager.BoldFamily, 10, FontStyle.Bold));

            _panel.AddElement(_backgroundTexture);
            _panel.AddElement(_priceText);
        }

        protected override void UpdateView()
        {
            base.UpdateView();
            var priceString = _manager.ItemPrice(CurrentItem).ToString();
            if (CurrentItem.HasAttribute(CommonAttributes.Amount)
                && CurrentItem.GetAttribute<int>(CommonAttributes.Amount) == int.MaxValue)
            {
                var clone = Item.FromArray(CurrentItem.ToArray());
                clone.SetAttribute(CommonAttributes.Amount, 1);
                priceString = _manager.ItemPrice(clone).ToString();
            }
            _priceText.Text = $"{priceString} G";
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

        public override Vector2 Position
        {
            get => base.Position;
            set
            {
                _backgroundTexture.Position += value - this.Position;
                _priceText.Position += value - this.Position;
                base.Position = value;
            }
        }

        public override bool Enabled
        {
            get => base.Enabled;
            set
            {
                base.Enabled = value;
                if (this.Enabled && CurrentItem != null)
                    _panel.Enable();
                else
                    _panel.Disable();
            }
        }
    }
}
