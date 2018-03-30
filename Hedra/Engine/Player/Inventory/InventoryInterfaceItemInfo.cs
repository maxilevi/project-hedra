﻿using System;
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
            this.ItemTexture = new Texture(0, BackgroundTexture.Position + BackgroundTexture.Scale * new Vector2(.45f, .0f) + Vector2.UnitY * -.05f,
                BackgroundTexture.Scale * .75f);

            this.ItemText = new RenderableText(string.Empty, BackgroundTexture.Position + Vector2.UnitY * .225f, Color.White,
                FontCache.Get(AssetManager.Fonts.Families[0], 13, FontStyle.Bold));
            DrawManager.UIRenderer.Add(ItemText, DrawOrder.After);

            this.ItemDescription = new RenderableText(string.Empty, BackgroundTexture.Position - Vector2.UnitY * .225f,
                Color.Bisque, FontCache.Get(AssetManager.Fonts.Families[0], 10, FontStyle.Bold));
            DrawManager.UIRenderer.Add(ItemDescription, DrawOrder.After);

            this._itemAttributes = new RenderableText(string.Empty, BackgroundTexture.Position - Vector2.UnitX * .025f + Vector2.UnitY * .05f,
                Color.White, FontCache.Get(AssetManager.Fonts.Families[0], 9, FontStyle.Bold));
            DrawManager.UIRenderer.Add(_itemAttributes, DrawOrder.After);

            _panel.AddElement(ItemText);
            _panel.AddElement(_itemAttributes);
            _panel.AddElement(ItemDescription);
            _panel.AddElement(ItemTexture);
            _panel.AddElement(BackgroundTexture);

            _nonWeaponItemAttributesPosition = Vector2.UnitY * -.175f;
            _nonWeaponItemTexturePosition = Vector2.UnitY * .0f;
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
                var tierColor = TierToColor(CurrentItem.Tier);
                ItemText.Color = tierColor;

                _itemAttributes.Color = tierColor;
                ItemDescription.Color = tierColor;
                _itemAttributes.Position = _weaponItemAttributesPosition + this.Position;
                ItemTexture.Position = _weaponItemTexturePosition + this.Position;
                ItemDescription.Text = Utils.FitString(CurrentItem.Description, 18);
            }
            else
            {
                ItemText.Color = Color.White;
                _itemAttributes.Color = Color.Bisque;
                ItemDescription.Color = Color.White;
                _itemAttributes.Position = _nonWeaponItemAttributesPosition + this.Position;
                ItemTexture.Position = _nonWeaponItemTexturePosition + this.Position;
                ItemDescription.Text = string.Empty;
            }

            ItemText.Text = Utils.FitString(CurrentItem.DisplayName.ToUpperInvariant(), 15);
            var attributes = CurrentItem.GetAttributes();
            var strBuilder = new StringBuilder();
            for (var i = 0; i < attributes.Length; i++)
            {
                if (!attributes[i].Hidden)
                {
                    var line = $"{attributes[i].Name.AddSpacesToSentence()}   ➝   {EscapeValue(attributes[i].Value)}";
                    strBuilder.AppendLine(line);
                }
            }
            _itemAttributes.Text = strBuilder.ToString();
            ItemTexture.TextureElement.IdPointer = () => _renderer.Draw(_currentItemMesh, CurrentItem,
                false, _currentItemMeshHeight * InventoryItemRenderer.ZOffsetFactor);
        }

        protected static string EscapeValue(object Value)
        {
            if (Value is double || Value is float) return ((float)Convert.ChangeType(Value, typeof(float))).ToString("0.0", CultureInfo.InvariantCulture);
            if (!(Value is int) && !(Value is long)) return Value.ToString();

            return (int) Convert.ChangeType(Value, typeof(int)) == int.MaxValue ? "∞" : Value.ToString();
        }

        private static Color TierToColor(ItemTier Tier)
        {
            return 
                Tier == ItemTier.Common ? Color.LightSkyBlue :
                Tier == ItemTier.Uncommon ? Color.LightGreen : 
                Tier == ItemTier.Rare ? Color.Bisque :
                Tier == ItemTier.Unique ? Color.MediumPurple :
                Tier == ItemTier.Legendary ? Color.DarkSalmon :
                Tier == ItemTier.Divine ? Color.MediumVioletRed : 
                Color.Transparent;
        }

        public void Show(Item Item)
        {
            if(Item == null || Item.IsGold) return;
            CurrentItem = Item;
            this.UpdateView();
            this.Enabled = true;
        }

        public void Hide()
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
            get { return BackgroundTexture.Position; }
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
            get { return _enabled; }
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
