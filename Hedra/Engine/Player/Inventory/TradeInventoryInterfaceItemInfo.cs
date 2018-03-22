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

        public TradeInventoryInterfaceItemInfo(InventoryItemRenderer Renderer) : base(Renderer)
        {
            _panel = new Panel();
            _backgroundTexture = new Texture("Assets/UI/InventoryBackground.png", Vector2.UnitY * -.35f, Vector2.One * .15f);
            _priceText = new GUIText(string.Empty, _backgroundTexture.Position, Color.Gold, FontCache.Get(AssetManager.Fonts.Families[0], 12, FontStyle.Bold));

            _panel.AddElement(_backgroundTexture);
            _panel.AddElement(_priceText);
        }

        protected override void UpdateView()
        {
            base.UpdateView();
            _priceText.Text = $"{TradeInventoryArrayInterfaceManager.ItemPrice(CurrentItem)} G";
        }

        public override Vector2 Position
        {
            get { return base.Position; }
            set
            {
                _backgroundTexture.Position += value - this.Position;
                _priceText.Position += value - this.Position;
                base.Position = value;
            }
        }

        public override bool Enabled
        {
            get { return base.Enabled; }
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
