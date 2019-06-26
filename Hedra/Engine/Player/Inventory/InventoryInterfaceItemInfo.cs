using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Game;
using Hedra.Localization;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.Player.Inventory
{
    public class InventoryInterfaceItemInfo
    {
        public static uint DefaultId { get; } = Graphics2D.LoadFromAssets("Assets/UI/InventoryItemInfo.png");
        public static Vector2 DefaultSize { get; } = Graphics2D.SizeFromAssets("Assets/UI/InventoryItemInfo.png").As1920x1080() * InventoryArrayInterface.UISizeMultiplier;
        protected Item CurrentItem;
        protected readonly BackgroundTexture BackgroundTexture;
        protected readonly BackgroundTexture ItemTexture;
        protected readonly RenderableText ItemText;
        protected readonly RenderableText ItemDescription;
        protected readonly RenderableText ItemAttributes;
        protected readonly BackgroundTexture HintTexture;
        protected readonly GUIText HintText;
        private readonly Vector2 _targetResolution = new Vector2(1366, 705);
        protected readonly Panel Panel;
        private readonly InventoryItemRenderer _renderer;
        private readonly Vector2 _weaponItemAttributesPosition;
        private readonly Vector2 _weaponItemTexturePosition;
        protected readonly Vector2 WeaponItemTextureScale;
        private readonly Vector2 _nonWeaponItemAttributesPosition;
        private readonly Vector2 _nonWeaponItemTexturePosition;
        private ObjectMesh _currentItemMesh;
        private Vector3 _currentItemMeshSize;
        private bool _enabled;

        public InventoryInterfaceItemInfo(InventoryItemRenderer Renderer)
        {
            this._renderer = Renderer;
            this.Panel = new Panel {DisableKeys = true};
            var resFactor = 1366f / GameSettings.Width;
            this.BackgroundTexture = new BackgroundTexture(DefaultId, Vector2.Zero, DefaultSize * .525f * resFactor);
            this.ItemTexture = new BackgroundTexture(0, BackgroundTexture.Position + Mathf.ScaleGui(_targetResolution, BackgroundTexture.Scale * new Vector2(.45f, .0f) + Vector2.UnitX * .025f),
                BackgroundTexture.Scale * .75f);

            this.ItemText = new RenderableText(string.Empty, Vector2.Zero, Color.White,
                FontCache.GetBold(13));
            DrawManager.UIRenderer.Add(ItemText, DrawOrder.After);

            this.ItemDescription = new RenderableText(string.Empty, BackgroundTexture.Position + Mathf.ScaleGui(_targetResolution, Vector2.UnitY * -.25f),
                Color.Bisque, FontCache.GetBold(10));
            DrawManager.UIRenderer.Add(ItemDescription, DrawOrder.After);

            this.ItemAttributes = new RenderableText(string.Empty, BackgroundTexture.Position + Mathf.ScaleGui(_targetResolution, Vector2.UnitX * -.05f + Vector2.UnitY * .15f),
                Color.White, FontCache.GetBold(10));
            DrawManager.UIRenderer.Add(ItemAttributes, DrawOrder.After);
            
            this.HintTexture = new BackgroundTexture(InventoryBackground.DefaultId, Vector2.UnitY * -.35f, InventoryBackground.DefaultSize * .15f);
            this.HintText = new GUIText(string.Empty, HintTexture.Position, Color.White, FontCache.GetBold(7.As1920x1080()));

            Panel.AddElement(HintTexture);
            Panel.AddElement(HintText);
            Panel.AddElement(ItemText);
            Panel.AddElement(ItemAttributes);
            Panel.AddElement(ItemDescription);
            Panel.AddElement(ItemTexture);
            Panel.AddElement(BackgroundTexture);

            _nonWeaponItemAttributesPosition = Mathf.ScaleGui(_targetResolution, Vector2.UnitY * -.225f);
            _nonWeaponItemTexturePosition = Mathf.ScaleGui(_targetResolution, Vector2.UnitY * .0f);
            _weaponItemAttributesPosition = ItemAttributes.Position;
            _weaponItemTexturePosition = ItemTexture.Position;
            WeaponItemTextureScale = ItemTexture.Scale;
        }

        protected virtual void UpdateView()
        {
            UpdateItemMesh();
            AddAttributes();
            AddLayout();
            AddTexture();
            AddHint();
            SetTitlePosition();
        }

        protected void SetTitlePosition()
        {
            ItemText.Position = BackgroundTexture.Position + BackgroundTexture.Scale.Y * Vector2.UnitY - ItemText.Scale.Y * Vector2.UnitY;
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
            ItemTexture.Scale = WeaponItemTextureScale;
            ItemDescription.Text = string.Empty;
            AccomodatePosition(ItemTexture);
            AccomodateScale(ItemTexture);
        }

        protected void AccomodatePosition(UIElement Element)
        {
            Element.Position += Mathf.ScaleGui(_targetResolution, Vector2.UnitY * DescriptionHeight);
        }
        
        protected void AccomodateScale(UIElement Element)
        {
            Element.Scale *= (1-(DescriptionHeight / WeaponItemTextureScale.Y));
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
            ItemTexture.Scale = WeaponItemTextureScale;
            ItemDescription.Text = Utils.FitString(CurrentItem.Description, 32);
        }

        protected virtual void UpdateItemMesh()
        {
            _currentItemMesh?.Dispose();
            _currentItemMesh = InventoryItemRenderer.BuildModel(CurrentItem.Model, out _currentItemMeshSize);
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
            ItemTexture.TextureElement.IdPointer = () => InventoryItemRenderer.Draw(_currentItemMesh, CurrentItem, false, _currentItemMeshSize);
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
            Panel.Disable();
            this.Enabled = false;
        }

        public bool Showing => CurrentItem != null;

        public Vector2 Scale => BackgroundTexture.Scale;

        public virtual Vector2 Position
        {
            get => BackgroundTexture.Position;
            set
            {
                var position = BackgroundTexture.Position;
                var elements = Panel.Elements.ToArray();
                for (var i = 0; i < elements.Length; ++i)
                {
                    elements[i].Position = elements[i].Position - position + value;
                }
            }
        }

        public virtual bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                if (_enabled && CurrentItem != null)
                    Panel.Enable();
                else
                    Panel.Disable();
                HintText.Disable();
                HintTexture.Disable();
            }
        }
    }
}
