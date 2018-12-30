using System;
using System.Drawing;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.Player.ToolbarSystem
{
    public class ToolbarInventoryInterface : InventoryArrayInterface
    {
        private readonly IPlayer _player;
        private readonly Panel _panel;
        private readonly RenderableTexture[] _textBackgrounds;
        private ObjectMesh _foodMesh;
        private Item _foodItem;
        private Item _builtFoodItem;
        private float _foodHeight;

        public ToolbarInventoryInterface(IPlayer Player, InventoryArray Array, int Offset, int Length, int SlotsPerLine, Vector2 Spacing, string[] CustomIcons = null) : base(Array, Offset, Length, SlotsPerLine, Spacing, CustomIcons)
        {
            _player = Player;
            _panel = new Panel();
            _textBackgrounds = new RenderableTexture[this.ButtonsText.Length];
            for (var i = 0; i < this.ButtonsText.Length; i++)
            {
                this.Array[i] = new Item
                {
                    Model = new VertexData()
                };
                if(i < Toolbar.InteractableItems) this.Array[i].SetAttribute("AbilityType", null);
                this.ButtonsText[i].Position = this.Buttons[i].Position + new Vector2(0, -InventoryArrayInterface.DefaultSize.Y) * .65f;
                this.ButtonsText[i].TextFont = FontCache.Get(AssetManager.BoldFamily, 9f, FontStyle.Bold);
                DrawManager.UIRenderer.Remove(this.ButtonsText[i]);
                _textBackgrounds[i] =
                    new RenderableTexture(new Texture("Assets/UI/InventoryCircle.png",
                        this.ButtonsText[i].Position, Vector2.One * .35f), DrawOrder.After);
                DrawManager.UIRenderer.Add(this.ButtonsText[i], DrawOrder.After);
                _panel.AddElement(_textBackgrounds[i]);
            }
            this.Buttons[this.Buttons.Length - 1].Texture.IdPointer = () => _foodItem != null
                ? Renderer.Draw(_foodMesh, _foodItem, true, _foodHeight * InventoryItemRenderer.ZOffsetFactor)
                : GUIRenderer.TransparentTexture;
        }

        public override void UpdateView()
        {
            for (var i = 0; i < _textBackgrounds.Length; i++)
            {
                if (Enabled)
                {
                    _textBackgrounds[i].Enable();
                }
                if (this.Array[i].HasAttribute("AbilityType") && this.Array[i].GetAttribute<Type>("AbilityType") == null)
                {
                    this.ButtonsText[i].Text = string.Empty;
                    _textBackgrounds[i].Disable();
                }
                else
                {
                    this.ButtonsText[i].Text = i < Toolbar.InteractableItems
                        ? (i + 1).ToString() : i == Toolbar.InteractableItems
                        ? "M1" : i == Toolbar.InteractableItems + 1 ? "M2" : string.Empty;
                }
            }
        }

        public void Update()
        {
            _foodItem = _player.Inventory.Food;
            if (_foodItem != _builtFoodItem && _foodItem != null)
            {
                _foodMesh = InventoryItemRenderer.BuildModel(_foodItem, out _foodHeight);
                _builtFoodItem = _foodItem;
            }
            this.ButtonsText[this.ButtonsText.Length - 1].Text =
                _player.Inventory.Food?.GetAttribute<int>(CommonAttributes.Amount).ToString() ?? string.Empty;
        }

        public override bool Enabled
        {
            get => base.Enabled;
            set
            {
                base.Enabled = value;
                if (this.Enabled) _panel.Enable();
                else _panel.Disable();
            }
        }

        public virtual Vector2 Scale
        {
            get => base.Scale;
            set
            {
                for (var i = 0; i < _textBackgrounds.Length; i++)
                {
                    _textBackgrounds[i].Scale = new Vector2(_textBackgrounds[i].Scale.X / base.IndividualScale.X,
                                                                 _textBackgrounds[i].Scale.Y / base.IndividualScale.Y) * value;

                    var relativePosition = _textBackgrounds[i].Position - Position;
                    _textBackgrounds[i].Position = new Vector2(relativePosition.X / base.Scale.X,
                                                                    relativePosition.Y / base.Scale.Y) * value + Position;
                }
                base.Scale = value;
            }
        }

        public virtual Vector2 IndividualScale
        {
            get => base.IndividualScale;
            set
            {
                for (var i = 0; i < _textBackgrounds.Length; i++)
                {
                    _textBackgrounds[i].Scale = new Vector2(_textBackgrounds[i].Scale.X / base.IndividualScale.X,
                                                    _textBackgrounds[i].Scale.Y / base.IndividualScale.Y) * value;
                }
                base.IndividualScale = value;
            }
        }

        public virtual Vector2 Position
        {
            get => base.Position;
            set
            {
                for (var i = 0; i < _textBackgrounds.Length; i++)
                {
                    _textBackgrounds[i].Position = _textBackgrounds[i].Position - base.Position + value;
                }
                base.Position = value;
            }
        }
    }
}
