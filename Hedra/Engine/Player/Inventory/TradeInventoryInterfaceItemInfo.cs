using System.Numerics;
using Hedra.API;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Rendering.UI;
using Hedra.Localization;
using Hedra.Rendering.UI;
using SixLabors.ImageSharp;

namespace Hedra.Engine.Player.Inventory
{
    public class TradeInventoryInterfaceItemInfo : InventoryInterfaceItemInfo
    {
        private readonly GUIText _topText;
        private readonly BackgroundTexture _topTexture;
        private TradeManager _manager;

        public TradeInventoryInterfaceItemInfo()
        {
            _topTexture = new BackgroundTexture(HintTexture.TextureElement.TextureId, Vector2.UnitY * .325f,
                HintTexture.Scale * 1.25f);
            _topText = new GUIText(string.Empty, _topTexture.Position, Color.White, FontCache.GetBold(10));

            HintText.TextFont = FontCache.GetBold(10);
            HintText.TextColor = Color.Gold;

            Panel.AddElement(_topText);
            Panel.AddElement(_topTexture);
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

            var classFromEquipment =
                CurrentItem.EquipmentType == null ? null : GetClassFromEquipment(CurrentItem.EquipmentType);
            if (classFromEquipment != null)
            {
                _topText.Text = Translations.Get("equipment_used_by", classFromEquipment);
                _topTexture.Enable();
                _topText.Enable();
            }
            else
            {
                _topTexture.Disable();
                _topText.Disable();
            }
        }

        private string GetClassFromEquipment(string Type)
        {
            switch (Type.ToLowerInvariant())
            {
                case "sword":
                case "axe":
                case "hammer":
                    return Translations.Get(Class.Warrior.ToString().ToLowerInvariant()).ToUpperInvariant();
                case "bow":
                case "knife":
                    return Translations.Get(Class.Archer.ToString().ToLowerInvariant()).ToUpperInvariant();
                case "claw":
                case "katar":
                case "doubleblades":
                    return Translations.Get(Class.Rogue.ToString().ToLowerInvariant()).ToUpperInvariant();
                case "staff":
                    return Translations.Get(Class.Mage.ToString().ToLowerInvariant()).ToUpperInvariant();
                default:
                    return null;
            }
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