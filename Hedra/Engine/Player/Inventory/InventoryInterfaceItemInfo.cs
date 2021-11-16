using System.Linq;
using System.Numerics;
using System.Text;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.UI;
using Hedra.Game;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using SixLabors.ImageSharp;

namespace Hedra.Engine.Player.Inventory
{
    public class InventoryInterfaceItemInfo
    {
        private readonly Vector2 _nonWeaponItemAttributesPosition;
        private readonly Vector2 _nonWeaponItemTexturePosition;
        private readonly Vector2 _targetResolution = new Vector2(1366, 705);
        private readonly Vector2 _weaponItemAttributesPosition;
        private readonly Vector2 _weaponItemTexturePosition;
        protected readonly BackgroundTexture BackgroundTexture;
        protected readonly GUIText HintText;
        protected readonly BackgroundTexture HintTexture;
        protected readonly RenderableText ItemAttributes;
        protected readonly RenderableText ItemDescription;
        protected readonly RenderableText ItemText;
        protected readonly BackgroundTexture ItemTexture;
        protected readonly Panel Panel;
        protected readonly Vector2 WeaponItemTextureScale;
        private ObjectMesh _currentItemMesh;
        private Vector3 _currentItemMeshSize;
        private bool _enabled;

        protected Item CurrentItem;

        public InventoryInterfaceItemInfo(float Scale = 1f)
        {
            Panel = new Panel { DisableKeys = true };
            var resFactor = 1366f / GameSettings.Width;
            BackgroundTexture = new BackgroundTexture(DefaultId, Vector2.Zero,
                (DefaultSize * .525f * resFactor * Scale).As1920x1080());
            ItemTexture = new BackgroundTexture(0,
                BackgroundTexture.Position + Mathf.ScaleGui(_targetResolution,
                    BackgroundTexture.Scale * new Vector2(.45f, .0f) + Vector2.UnitX * .025f),
                BackgroundTexture.Scale * .75f);

            ItemText = new RenderableText(string.Empty, Vector2.Zero, Color.White, FontCache.GetBold(13));
            DrawManager.UIRenderer.Add(ItemText, DrawOrder.After);

            ItemDescription = new RenderableText(string.Empty,
                BackgroundTexture.Position + Mathf.ScaleGui(_targetResolution, Vector2.UnitY * -.25f),
                Color.Bisque, FontCache.GetBold(10));
            DrawManager.UIRenderer.Add(ItemDescription, DrawOrder.After);

            ItemAttributes = new RenderableText(string.Empty,
                BackgroundTexture.Position +
                Mathf.ScaleGui(_targetResolution, Vector2.UnitX * -.05f + Vector2.UnitY * .15f),
                Color.White, FontCache.GetBold(10));
            DrawManager.UIRenderer.Add(ItemAttributes, DrawOrder.After);

            HintTexture = new BackgroundTexture(InventoryBackground.DefaultId, Vector2.UnitY * -.305f,
                InventoryBackground.DefaultSize * .175f);
            HintText = new GUIText(string.Empty, HintTexture.Position, Color.White,
                FontCache.GetBold(10.As1920x1080()));

            Panel.AddElement(HintTexture);
            Panel.AddElement(HintText);
            Panel.AddElement(ItemText);
            Panel.AddElement(ItemAttributes);
            Panel.AddElement(ItemDescription);
            Panel.AddElement(ItemTexture);
            Panel.AddElement(BackgroundTexture);

            _nonWeaponItemAttributesPosition = Mathf.ScaleGui(_targetResolution, Vector2.UnitY * -.225f).As1920x1080();
            _nonWeaponItemTexturePosition = Mathf.ScaleGui(_targetResolution, Vector2.UnitY * .0f).As1920x1080();
            _weaponItemAttributesPosition = ItemAttributes.Position;
            _weaponItemTexturePosition = ItemTexture.Position;
            WeaponItemTextureScale = ItemTexture.Scale;
        }

        public static uint DefaultId { get; } = Graphics2D.LoadFromAssets("Assets/UI/InventoryItemInfo.png");

        public static Vector2 DefaultSize { get; } =
            Graphics2D.SizeFromAssets("Assets/UI/InventoryItemInfo.png").As1920x1080() *
            InventoryArrayInterface.UISizeMultiplier;

        protected virtual float DescriptionHeight => ItemAttributes.UIText.Scale.Y;

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
                    elements[i].Position = elements[i].Position - position + value;
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
            ItemText.Position = BackgroundTexture.Position + BackgroundTexture.Scale.Y * Vector2.UnitY -
                                ItemText.Scale.Y * Vector2.UnitY;
        }

        protected virtual void AddHint()
        {
            if (CurrentItem.IsConsumable)
            {
                HintText.Enable();
                HintTexture.Enable();
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
                if (CurrentItem.IsCompanion) MakeColorOfTier();
            }
        }

        protected void AddNormalLayout()
        {
            ItemText.Color = Color.White;
            ItemAttributes.Color = Color.White;
            ItemDescription.Color = Color.White;
            ItemText.Text = Utils.FitString(CurrentItem.DisplayName, 20);
            ItemAttributes.Position = _nonWeaponItemAttributesPosition + Position;
            ItemTexture.Position = _nonWeaponItemTexturePosition + Position;
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
            Element.Scale *= 1 - DescriptionHeight / WeaponItemTextureScale.Y;
        }

        protected void AddEquipmentLayout()
        {
            MakeColorOfTier();
            ItemText.Text =
                Utils.FitString(
                    $"{Translations.Get(CurrentItem.Tier.ToString().ToLowerInvariant())} {CurrentItem.DisplayName}",
                    20);
            ItemAttributes.Position = _weaponItemAttributesPosition + Position;
            ItemTexture.Position = _weaponItemTexturePosition + Position;
            ItemTexture.Scale = WeaponItemTextureScale;
            ItemDescription.Text = Utils.FitString(CurrentItem.Description, 32);
        }

        private void MakeColorOfTier()
        {
            var tierColor = ItemUtils.TierToColor(CurrentItem.Tier);
            ItemText.Color = tierColor;
            ItemAttributes.Color = tierColor;
            ItemDescription.Color = tierColor;
        }

        protected virtual void UpdateItemMesh()
        {
            UpdateItemMesh(CurrentItem.Model);
        }

        protected void UpdateItemMesh(VertexData Model)
        {
            _currentItemMesh?.Dispose();
            _currentItemMesh = InventoryItemRenderer.BuildModel(Model, out _currentItemMeshSize);
        }

        protected virtual void AddAttributes()
        {
            var attributes = CurrentItem.GetAttributes();
            var strBuilder = new StringBuilder();
            for (var i = 0; i < attributes.Length; i++)
                if (!attributes[i].Hidden || GameSettings.DebugView)
                    strBuilder.AppendLine(AttributeFormatter.Format(attributes[i]));
            if (GameSettings.DebugView && CurrentItem.HasAttribute(CommonAttributes.Damage))
                strBuilder.AppendLine($"Modifier   ➝   {GameManager.Player.WeaponModifier(CurrentItem)}");
            ItemAttributes.Text = strBuilder.ToString();
        }

        protected virtual void AddTexture()
        {
            ItemTexture.TextureElement.IdPointer = () =>
                InventoryItemRenderer.Draw(_currentItemMesh, CurrentItem, false, _currentItemMeshSize);
        }

        public virtual void Show(Item Item)
        {
            if (Item == null) return;
            CurrentItem = Item;
            Enabled = true;
            UpdateView();
        }

        public virtual void Hide()
        {
            if (CurrentItem == null) return;
            _currentItemMesh?.Dispose();
            _currentItemMesh = null;
            CurrentItem = null;
            Panel.Disable();
            Enabled = false;
        }
    }
}