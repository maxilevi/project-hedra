using System;
using System.Drawing;
using System.Globalization;
using System.Text;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Player.Inventory
{
    public class InventoryInterfaceItemInfo
    {
        protected Item CurrentItem;
        protected readonly Texture BackgroundTexture;
        protected readonly Texture ItemTexture;
        protected readonly RenderableText ItemText;
        protected readonly RenderableText ItemDescription;
        private readonly Vector2 _targetResolution = new Vector2(1366, 705);
        private readonly Panel _panel;
        private readonly InventoryItemRenderer _renderer;
        private readonly RenderableText _itemAttributes;
        private readonly Vector2 _weaponItemAttributesPosition;
        private readonly Vector2 _weaponItemTexturePosition;
        private readonly Vector2 _nonWeaponItemAttributesPosition;
        private readonly Vector2 _nonWeaponItemTexturePosition;
        private ObjectMesh _currentItemMesh;
        private float _currentItemMeshHeight;
        private bool _enabled;

        public InventoryInterfaceItemInfo(InventoryItemRenderer Renderer)
        {
            this._renderer = Renderer;
            this._panel = new Panel();
            this.BackgroundTexture = new Texture("Assets/UI/InventoryItemInfo.png", Vector2.Zero, Vector2.One * .35f);
            this.ItemTexture = new Texture(0, BackgroundTexture.Position + Mathf.ScaleGUI(_targetResolution, BackgroundTexture.Scale * new Vector2(.45f, .0f) + Vector2.UnitY * -.05f),
                BackgroundTexture.Scale * .75f);

            this.ItemText = new RenderableText(string.Empty, BackgroundTexture.Position + Mathf.ScaleGUI(_targetResolution, Vector2.UnitY * .225f), Color.White,
                FontCache.Get(AssetManager.BoldFamily, 13, FontStyle.Bold));
            DrawManager.UIRenderer.Add(ItemText, DrawOrder.After);

            this.ItemDescription = new RenderableText(string.Empty, BackgroundTexture.Position - Mathf.ScaleGUI(_targetResolution, Vector2.UnitY * .2f),
                Color.Bisque, FontCache.Get(AssetManager.BoldFamily, 10, FontStyle.Bold));
            DrawManager.UIRenderer.Add(ItemDescription, DrawOrder.After);

            this._itemAttributes = new RenderableText(string.Empty, BackgroundTexture.Position - Mathf.ScaleGUI(_targetResolution, Vector2.UnitX * .025f + Vector2.UnitY * .05f),
                Color.White, FontCache.Get(AssetManager.BoldFamily, 9, FontStyle.Bold));
            DrawManager.UIRenderer.Add(_itemAttributes, DrawOrder.After);

            _panel.AddElement(ItemText);
            _panel.AddElement(_itemAttributes);
            _panel.AddElement(ItemDescription);
            _panel.AddElement(ItemTexture);
            _panel.AddElement(BackgroundTexture);

            _nonWeaponItemAttributesPosition = Mathf.ScaleGUI(_targetResolution, Vector2.UnitY * -.175f);
            _nonWeaponItemTexturePosition = Mathf.ScaleGUI(_targetResolution, Vector2.UnitY * .0f);
            _weaponItemAttributesPosition = _itemAttributes.Position;
            _weaponItemTexturePosition = ItemTexture.Position;
        }

        protected virtual void UpdateView()
        {
            _currentItemMesh?.Dispose();
            _currentItemMesh = _renderer.BuildModel(CurrentItem, out _currentItemMeshHeight);
            var isEquipment = CurrentItem.IsEquipment;
            if (isEquipment)
            {
                var tierColor = ItemUtils.TierToColor(CurrentItem.Tier);
                ItemText.Color = tierColor;

                ItemText.Text = Utils.FitString($"{CurrentItem.Tier} {CurrentItem.DisplayName}", 15);
                _itemAttributes.Color = tierColor;
                ItemDescription.Color = tierColor;
                _itemAttributes.Position = _weaponItemAttributesPosition + this.Position;
                ItemTexture.Position = _weaponItemTexturePosition + this.Position;
                ItemDescription.Text = Utils.FitString(CurrentItem.Description, 18);
            }
            else
            {
                ItemText.Color = Color.White;
                ItemText.Text = Utils.FitString(CurrentItem.DisplayName, 15);
                _itemAttributes.Color = Color.Bisque;
                ItemDescription.Color = Color.White;
                _itemAttributes.Position = _nonWeaponItemAttributesPosition + this.Position;
                ItemTexture.Position = _nonWeaponItemTexturePosition + this.Position;
                ItemDescription.Text = string.Empty;
            }

            var attributes = CurrentItem.GetAttributes();
            var strBuilder = new StringBuilder();
            for (var i = 0; i < attributes.Length; i++)
            {
                if (!attributes[i].Hidden || GameSettings.DebugView)
                {
                    var line = $"{attributes[i].Name.AddSpacesToSentence()}   ➝   {Format(attributes[i].Display, attributes[i].Value)}";
                    strBuilder.AppendLine(line);
                }
            }
            if (GameSettings.DebugView && CurrentItem.HasAttribute(CommonAttributes.Damage))
            {
                strBuilder.AppendLine($"Modifier   ➝   {GameManager.Player.WeaponModifier(CurrentItem)}");
            }
            _itemAttributes.Text = strBuilder.ToString();
            ItemTexture.TextureElement.IdPointer = () => _renderer.Draw(_currentItemMesh, CurrentItem,
                false, _currentItemMeshHeight * InventoryItemRenderer.ZOffsetFactor);
        }

        protected static object Format(string Display, object Value)
        {
            if (Value is double || Value is float)
            {
                var asNumber = (float) Convert.ChangeType(Value, typeof(float));
                if (Display == null) return asNumber.ToString("0.00", CultureInfo.InvariantCulture);
                switch ((AttributeDisplay) Enum.Parse(typeof(AttributeDisplay), Display))
                {
                    case AttributeDisplay.Percentage:
                        return $"{(asNumber > 0 ? "+" : asNumber == 0 ? string.Empty : "-")}{ (int) (asNumber * 100f)}%";
                    case AttributeDisplay.Flat:
                        return asNumber.ToString("0.00", CultureInfo.InvariantCulture);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            if (!(Value is int) && !(Value is long)) return Value.ToString();
            return (int)Convert.ChangeType(Value, typeof(int)) == int.MaxValue ? "∞" : Value.ToString();
        }

        public virtual void Show(Item Item)
        {
            if(Item == null) return;
            CurrentItem = Item;
            this.UpdateView();
            this.Enabled = true;
        }

        public virtual void Hide()
        {
            if(CurrentItem == null) return;
            _currentItemMesh?.Dispose();
            _currentItemMesh = null;
            CurrentItem = null;
            _panel.Disable();
            this.Enabled = false;
        }

        public bool Showing => CurrentItem != null;

        public Vector2 Scale => BackgroundTexture.Scale;

        public virtual Vector2 Position
        {
            get => BackgroundTexture.Position;
            set
            {
                ItemTexture.Position = ItemTexture.Position - Position + value;
                ItemText.Position = ItemText.Position - Position + value;
                ItemDescription.Position = ItemDescription.Position - Position + value;
                _itemAttributes.Position = _itemAttributes.Position - Position + value;
                BackgroundTexture.Position = value;
            }
        }

        public virtual bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                if (_enabled && CurrentItem != null)
                    _panel.Enable();
                else
                    _panel.Disable();
            }
        }
    }
}
