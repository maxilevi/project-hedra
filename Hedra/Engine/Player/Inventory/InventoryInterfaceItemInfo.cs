using System.Drawing;
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
        private readonly Texture _backgroundTexture;
        private readonly Texture _itemTexture;
        private readonly Panel _panel;
        private readonly InventoryItemRenderer _renderer;
        private readonly RenderableText _itemText;
        private readonly RenderableText _itemDescription;
        private readonly RenderableText _itemAttributes;
        private readonly Vector2 _weaponItemAttributesPosition;
        private readonly Vector2 _weaponItemTexturePosition;
        private readonly Vector2 _nonWeaponItemAttributesPosition;
        private readonly Vector2 _nonWeaponItemTexturePosition;
        private EntityMesh _currentItemMesh;
        private Item _currentItem;
        private bool _enabled;

        public InventoryInterfaceItemInfo(InventoryItemRenderer Renderer)
        {
            this._renderer = Renderer;
            this._panel = new Panel();
            this._backgroundTexture = new Texture("Assets/UI/InventoryItemInfo.png", Vector2.Zero, Vector2.One * .35f);
            this._itemTexture = new Texture(0, _backgroundTexture.Position + _backgroundTexture.Scale * new Vector2(.45f, .0f) + Vector2.UnitY * -.05f,
                _backgroundTexture.Scale * .75f);

            this._itemText = new RenderableText(string.Empty, _backgroundTexture.Position + Vector2.UnitY * .225f, Color.White,
                FontCache.Get(AssetManager.Fonts.Families[0], 13, FontStyle.Bold));
            DrawManager.UIRenderer.Add(_itemText, DrawOrder.After);

            this._itemDescription = new RenderableText(string.Empty, _backgroundTexture.Position - Vector2.UnitY * .225f,
                Color.Bisque, FontCache.Get(AssetManager.Fonts.Families[0], 10, FontStyle.Bold));
            DrawManager.UIRenderer.Add(_itemDescription, DrawOrder.After);

            this._itemAttributes = new RenderableText(string.Empty, _backgroundTexture.Position - Vector2.UnitX * .025f + Vector2.UnitY * .05f,
                Color.White, FontCache.Get(AssetManager.Fonts.Families[0], 9, FontStyle.Bold));
            DrawManager.UIRenderer.Add(_itemAttributes, DrawOrder.After);

            _panel.AddElement(_itemText);
            _panel.AddElement(_itemAttributes);
            _panel.AddElement(_itemDescription);
            _panel.AddElement(_itemTexture);
            _panel.AddElement(_backgroundTexture);

            _nonWeaponItemAttributesPosition = Vector2.UnitY * -.175f;
            _nonWeaponItemTexturePosition = Vector2.UnitY * .0f;
            _weaponItemAttributesPosition = _itemAttributes.Position;
            _weaponItemTexturePosition = _itemTexture.Position;
        }

        private void UpdateView()
        {
            var isEquipment = _currentItem.EquipmentType != null;
            if (isEquipment)
            {
                var tierColor = this.TierToColor(_currentItem.Tier);
                _itemText.Color = tierColor;

                _itemAttributes.Color = tierColor;
                _itemDescription.Color = tierColor;
                _itemAttributes.Position = _weaponItemAttributesPosition + this.Position;
                _itemTexture.Position = _weaponItemTexturePosition + this.Position;
                _itemDescription.Text = Utils.FitString(_currentItem.Description, 25);
            }
            else
            {
                _itemText.Color = Color.White;
                _itemAttributes.Color = Color.Bisque;
                _itemDescription.Color = Color.White;
                _itemAttributes.Position = _nonWeaponItemAttributesPosition + this.Position;
                _itemTexture.Position = _nonWeaponItemTexturePosition + this.Position;
                _itemDescription.Text = string.Empty;
            }

            _itemText.Text = _currentItem.DisplayName.ToUpperInvariant();
            var attributes = _currentItem.GetAttributes();
            var strBuilder = new StringBuilder();
            for (var i = 0; i < attributes.Length; i++)
            {
                if (!attributes[i].Hidden)
                {
                    var line = $"{attributes[i].Name.AddSpacesToSentence()}   ➝   {attributes[i].Value.ToString()}";
                    strBuilder.AppendLine(line);
                }
            }
            _itemAttributes.Text = strBuilder.ToString();
            var model = _currentItem.Model;
            float newOffset = model.SupportPoint(Vector3.UnitY).Y - model.SupportPoint(-Vector3.UnitY).Y;
            _itemTexture.TextureElement.IdPointer = () => _renderer.Draw(_currentItemMesh, _currentItem,
                false, newOffset * InventoryItemRenderer.ZOffsetFactor);
        }

        private Color TierToColor(ItemTier Tier)
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
            if(Item == null) return;
            _currentItem = Item;
            _currentItemMesh = EntityMesh.FromVertexData(Item.Model);
            _currentItemMesh.UseFog = false;
            this.UpdateView();
            _panel.Enable();
        }

        public void Hide()
        {
            if(_currentItem == null) return;
            _currentItemMesh.Dispose();
            _currentItemMesh = null;
            _currentItem = null;
            _panel.Disable();
        }

        public bool Showing => _currentItem != null;

        public Vector2 Scale => _backgroundTexture.Scale;

        public Vector2 Position
        {
            get { return _backgroundTexture.Position; }
            set
            {
                _itemTexture.Position = _itemTexture.Position - Position + value;
                _itemText.Position = _itemText.Position - Position + value;
                _itemDescription.Position = _itemDescription.Position - Position + value;
                _itemAttributes.Position = _itemAttributes.Position - Position + value;
                _backgroundTexture.Position = value;
            }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                if (_enabled && _currentItem != null)
                    _panel.Enable();
                else
                    _panel.Disable();
            }
        }
    }
}
