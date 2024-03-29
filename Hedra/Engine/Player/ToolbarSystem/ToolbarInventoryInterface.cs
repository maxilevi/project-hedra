using System;
using System.Numerics;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.Rendering.UI;

namespace Hedra.Engine.Player.ToolbarSystem
{
    public class ToolbarInventoryInterface : InventoryArrayInterface
    {
        private readonly Panel _panel;
        private readonly IPlayer _player;
        private readonly RenderableTexture[] _textBackgrounds;
        private Item _builtFoodItem;
        private Item _foodItem;
        private ObjectMesh _foodMesh;
        private Vector3 _foodSize;

        public ToolbarInventoryInterface(IPlayer Player, InventoryArray Array, int Offset, int Length, int SlotsPerLine,
            Vector2 Spacing, string[] CustomIcons = null) : base(Array, Offset, Length, SlotsPerLine, Spacing,
            CustomIcons)
        {
            _player = Player;
            _panel = new Panel();
            _textBackgrounds = new RenderableTexture[ButtonsText.Length];
            _player.Inventory.InventoryUpdated += OnInventoryUpdated;
            for (var i = 0; i < ButtonsText.Length; i++)
            {
                this.Array[i] = new Item
                {
                    Model = new VertexData()
                };
                if (i < Toolbar.InteractableItems) this.Array[i].SetAttribute("AbilityType", null);
                ButtonsText[i].Position = Buttons[i].Position + new Vector2(0, -DefaultSize.Y) * .65f;
                ButtonsText[i].TextFont = FontCache.GetBold(9f);
                DrawManager.UIRenderer.Remove(ButtonsText[i]);
                _textBackgrounds[i] =
                    new RenderableTexture(new BackgroundTexture("Assets/UI/InventoryCircle.png",
                        ButtonsText[i].Position, (Vector2.One * .35f).As1920x1080()), DrawOrder.After);
                DrawManager.UIRenderer.Add(ButtonsText[i], DrawOrder.After);
                _panel.AddElement(_textBackgrounds[i]);
            }

            Buttons[Buttons.Length - 1].Texture.IdPointer = () => _foodItem != null
                ? InventoryItemRenderer.Draw(_foodMesh, _foodItem, true, _foodSize)
                : GUIRenderer.TransparentTexture;
        }

        public override bool Enabled
        {
            get => base.Enabled;
            set
            {
                base.Enabled = value;
                if (Enabled) _panel.Enable();
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
                    _textBackgrounds[i].Scale = new Vector2(_textBackgrounds[i].Scale.X / base.IndividualScale.X,
                        _textBackgrounds[i].Scale.Y / base.IndividualScale.Y) * value;
                base.IndividualScale = value;
            }
        }

        public virtual Vector2 Position
        {
            get => base.Position;
            set
            {
                for (var i = 0; i < _textBackgrounds.Length; i++)
                    _textBackgrounds[i].Position = _textBackgrounds[i].Position - base.Position + value;
                base.Position = value;
            }
        }

        public override void UpdateView()
        {
            for (var i = 0; i < _textBackgrounds.Length; i++)
            {
                if (Enabled) _textBackgrounds[i].Enable();
                if (Array[i].HasAttribute("AbilityType") && Array[i].GetAttribute<Type>("AbilityType") == null)
                {
                    ButtonsText[i].Text = string.Empty;
                    _textBackgrounds[i].Disable();
                }
                else
                {
                    ButtonsText[i].Text = i < Toolbar.InteractableItems
                        ? (i + 1).ToString()
                        : i == Toolbar.InteractableItems
                            ? "M1"
                            : i == Toolbar.InteractableItems + 1
                                ? "M2"
                                : Controls.Eat.ToString().ToUpperInvariant();
                }
            }
        }

        public void Update()
        {
            _foodItem = _player.Inventory.Food;
            if (_foodItem != _builtFoodItem && _foodItem != null)
            {
                _foodMesh = InventoryItemRenderer.BuildModel(_foodItem.Model, out _foodSize);
                _builtFoodItem = _foodItem;
            }
        }

        private void OnInventoryUpdated()
        {
            //this.ButtonsText[this.ButtonsText.Length - 1].Text = _player.Inventory.Food?.GetAttribute<int>(CommonAttributes.Amount).ToString() ?? string.Empty;
        }
    }
}