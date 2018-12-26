using System;
using System.Drawing;
using System.Globalization;
using System.Text;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
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
        protected readonly RenderableText ItemAttributes;
        protected readonly Texture HintTexture;
        protected readonly GUIText HintText;
        protected readonly Vector2 TargetResolution = new Vector2(1366, 705);
        private readonly Panel _panel;
        private readonly InventoryItemRenderer _renderer;
        private readonly Vector2 _weaponItemAttributesPosition;
        private readonly Vector2 _weaponItemTexturePosition;
        private readonly Vector2 _weaponItemTextureScale;
        private readonly Vector2 _nonWeaponItemAttributesPosition;
        private readonly Vector2 _nonWeaponItemTexturePosition;
        private ObjectMesh _currentItemMesh;
        private float _currentItemMeshHeight;
        private bool _enabled;

        public InventoryInterfaceItemInfo(InventoryItemRenderer Renderer)
        {
            this._renderer = Renderer;
            this._panel = new Panel();
            this.BackgroundTexture = new Texture("Assets/UI/InventoryItemInfo.png", Vector2.Zero, Vector2.One * .45f);
            this.ItemTexture = new Texture(0, BackgroundTexture.Position + Mathf.ScaleGui(TargetResolution, BackgroundTexture.Scale * new Vector2(.45f, .0f) + Vector2.UnitX * .025f),
                BackgroundTexture.Scale * .75f);

            this.ItemText = new RenderableText(string.Empty, BackgroundTexture.Position + Mathf.ScaleGui(TargetResolution, Vector2.UnitY * .325f), Color.White,
                FontCache.Get(AssetManager.BoldFamily, 13, FontStyle.Bold));
            DrawManager.UIRenderer.Add(ItemText, DrawOrder.After);

            this.ItemDescription = new RenderableText(string.Empty, BackgroundTexture.Position + Mathf.ScaleGui(TargetResolution, Vector2.UnitY * -.25f),
                Color.Bisque, FontCache.Get(AssetManager.BoldFamily, 10, FontStyle.Bold));
            DrawManager.UIRenderer.Add(ItemDescription, DrawOrder.After);

            this.ItemAttributes = new RenderableText(string.Empty, BackgroundTexture.Position + Mathf.ScaleGui(TargetResolution, Vector2.UnitX * -.05f + Vector2.UnitY * .15f),
                Color.White, FontCache.Get(AssetManager.BoldFamily, 10, FontStyle.Bold));
            DrawManager.UIRenderer.Add(ItemAttributes, DrawOrder.After);
            
            this.HintTexture = new Texture("Assets/UI/InventoryBackground.png", Vector2.UnitY * -.35f, Vector2.One * .15f);
            this.HintText = new GUIText(string.Empty, HintTexture.Position, Color.White, FontCache.Get(AssetManager.BoldFamily, 7, FontStyle.Bold));

            _panel.AddElement(HintTexture);
            _panel.AddElement(HintText);
            _panel.AddElement(ItemText);
            _panel.AddElement(ItemAttributes);
            _panel.AddElement(ItemDescription);
            _panel.AddElement(ItemTexture);
            _panel.AddElement(BackgroundTexture);

            _nonWeaponItemAttributesPosition = Mathf.ScaleGui(TargetResolution, Vector2.UnitY * -.225f);
            _nonWeaponItemTexturePosition = Mathf.ScaleGui(TargetResolution, Vector2.UnitY * .0f);
            _weaponItemAttributesPosition = ItemAttributes.Position;
            _weaponItemTexturePosition = ItemTexture.Position;
            _weaponItemTextureScale = ItemTexture.Scale;
        }

        protected virtual void UpdateView()
        {
            UpdateItemMesh();
            AddAttributes();
            AddLayout();
            AddTexture();
            AddHint();
        }

        protected virtual void AddHint()
        {
            if (CurrentItem.IsConsumable)
            {
                HintText.Text = Translations.Get("right_click_use");
            }
            else
            {
                HintText.Disable();
                HintTexture.Disable();
            }
        }

        protected virtual void AddLayout()
        {
            if (CurrentItem.IsEquipment)
            {
                AddEquipmentLayout();
            }
            else
            {
                AddNormalLayout();
            }
        }

        protected void AddNormalLayout()
        {
            ItemText.Color = Color.White;
            ItemAttributes.Color = Color.White;
            ItemDescription.Color = Color.White;
            ItemText.Text = Utils.FitString(CurrentItem.DisplayName, 20);
            ItemAttributes.Position = _nonWeaponItemAttributesPosition + this.Position;
            ItemTexture.Position = _nonWeaponItemTexturePosition + this.Position;               
            ItemTexture.Scale = _weaponItemTextureScale;
            ItemDescription.Text = string.Empty;
            AccomodatePosition(ItemTexture);
            AccomodateScale(ItemTexture);
        }

        protected void AccomodatePosition(UIElement Element)
        {
            Element.Position += Mathf.ScaleGui(TargetResolution, Vector2.UnitY * DescriptionHeight);
        }
        
        protected void AccomodateScale(UIElement Element)
        {
            Element.Scale *= (1-(DescriptionHeight / _weaponItemTextureScale.Y));
        }
        
        protected virtual float DescriptionHeight => ItemAttributes.UIText.Scale.Y;

        protected void AddEquipmentLayout()
        {
            var tierColor = ItemUtils.TierToColor(CurrentItem.Tier);
            ItemText.Color = tierColor;

            ItemText.Text = Utils.FitString($"{CurrentItem.Tier} {CurrentItem.DisplayName}", 20);
            ItemAttributes.Color = tierColor;
            ItemDescription.Color = tierColor;
            ItemAttributes.Position = _weaponItemAttributesPosition + this.Position;
            ItemTexture.Position = _weaponItemTexturePosition + this.Position;
            ItemTexture.Scale = _weaponItemTextureScale;
            ItemDescription.Text = Utils.FitString(CurrentItem.Description, 32);
        }

        protected virtual void UpdateItemMesh()
        {
            _currentItemMesh?.Dispose();
            _currentItemMesh = _renderer.BuildModel(CurrentItem, out _currentItemMeshHeight);
        }

        protected virtual void AddAttributes()
        {
            var attributes = CurrentItem.GetAttributes();
            var strBuilder = new StringBuilder();
            for (var i = 0; i < attributes.Length; i++)
            {
                if (!attributes[i].Hidden || GameSettings.DebugView)
                {
                    strBuilder.AppendLine(AttributeFormatter.Format(attributes[i]));
                }
            }
            if (GameSettings.DebugView && CurrentItem.HasAttribute(CommonAttributes.Damage))
            {
                strBuilder.AppendLine($"Modifier   ➝   {GameManager.Player.WeaponModifier(CurrentItem)}");
            }
            ItemAttributes.Text = strBuilder.ToString();
        }

        protected virtual void AddTexture()
        {
            ItemTexture.TextureElement.IdPointer = () => _renderer.Draw(_currentItemMesh, CurrentItem,
                false, _currentItemMeshHeight * InventoryItemRenderer.ZOffsetFactor);
        }
        
        public virtual void Show(Item Item)
        {
            if(Item == null) return;
            CurrentItem = Item;
            this.Enabled = true;
            this.UpdateView();
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
                HintTexture.Position = HintTexture.Position - Position + value;
                HintText.Position = HintText.Position - Position + value;
                ItemTexture.Position = ItemTexture.Position - Position + value;
                ItemText.Position = ItemText.Position - Position + value;
                ItemDescription.Position = ItemDescription.Position - Position + value;
                ItemAttributes.Position = ItemAttributes.Position - Position + value;
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
                HintText.Disable();
                HintTexture.Disable();
            }
        }
    }
}
