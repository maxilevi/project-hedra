using System;
using System.Drawing;
using System.Linq;
using Hedra.Engine.Events;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using OpenToolkit.Mathematics;
using OpenTK.Input;

namespace Hedra.Engine.Player.Inventory
{
    public class InventoryArrayInterface
    {
        public const string DefaultIcon = "Assets/UI/InventorySlot.png";
        public const float UISizeMultiplier = 1.15f;
        public static uint DefaultId { get; } = Graphics2D.LoadFromAssets(DefaultIcon);
        public static Vector2 DefaultSize { get; } = Graphics2D.SizeFromAssets(DefaultIcon).As1920x1080() * UISizeMultiplier;
        private InventoryArray _array;
        private readonly InventoryItemRenderer _renderer;
        private readonly BackgroundTexture[] _inventoryTextures;
        private readonly RenderableButton[] _inventoryButtons;
        private readonly RenderableText[] _inventoryButtonsText;
        protected readonly Panel _panel;
        private readonly int _length;
        private readonly int _offset;
        private Vector2 _position = Vector2.Zero;
        private Vector2 _individualScale = Vector2.One;
        private Vector2 _scale = Vector2.One;
        private bool _enabled;

        public InventoryArrayInterface(InventoryArray Array, int Offset, int Length, int SlotsPerLine,
            Vector2 Spacing, string[] CustomIcons = null)
        {
            this._array = Array;
            this._length = Length;
            this._offset = Offset;
            this._renderer = new InventoryItemRenderer(_array, _offset, _length);
            this._inventoryTextures = new BackgroundTexture[_length];
            this._inventoryButtons = new RenderableButton[_length];
            this._inventoryButtonsText = new RenderableText[_length];
            this._panel = new Panel{DisableKeys = true};
            var size = DefaultSize;
            var offset = new Vector2(size.X, size.Y);
            var slotsPerLine = Math.Max(SlotsPerLine, 1);
            var wholeSize = new Vector2(
                size.X * (_length - 1 - (_length - 1) / slotsPerLine * slotsPerLine),
                size.Y * ((_length - 1) / (float)slotsPerLine)
            );
            var e = Math.Max(SlotsPerLine * SlotsPerLine, 1);
            for (var i = 0; i < _length; i++)
            {
                var k = i;
                var j = i / SlotsPerLine;
                var scale = Vector2.One * .5f;
                var position = Vector2.Zero + new Vector2((i - j * SlotsPerLine) * offset.X, j * offset.Y) * Spacing -
                               wholeSize * .5f;
                var customId = CustomIcons != null ? Graphics2D.LoadFromAssets(CustomIcons[i]) : GUIRenderer.TransparentTexture;
                var customScale = /*CustomIcons != null ? Graphics2D.SizeFromAssets(CustomIcons[i]) : */InventoryArrayInterface.DefaultSize;

                _inventoryTextures[i] = new BackgroundTexture(CustomIcons != null ? customId : DefaultId, position, customScale * scale);
                _inventoryButtonsText[i] = new RenderableText(string.Empty, position + new Vector2(size.X, -size.Y) * .25f, Color.White, FontCache.GetBold(10));
                _inventoryButtons[i] = new RenderableButton(position, size * scale * .8f, GUIRenderer.TransparentTexture);
                _inventoryButtons[i].Texture.IdPointer = () => _renderer.Draw(k);
                _inventoryButtons[i].PlaySound = false;
                _panel.AddElement(_inventoryTextures[i]);
                _panel.AddElement(_inventoryButtonsText[i]);
                _panel.AddElement(_inventoryButtons[i]);

                DrawManager.UIRenderer.Add(_inventoryButtons[i], DrawOrder.After);
                DrawManager.UIRenderer.Add(_inventoryButtonsText[i], DrawOrder.After);
            }
        }

        public virtual void UpdateView()
        {
            for (var i = 0; i < _length; i++)
            {
                if (_array[i] == null || !_array[i].HasAttribute(CommonAttributes.Amount))
                    _inventoryButtonsText[i].Text = string.Empty;
                else
                {
                    var amount = _array[i].GetAttribute<int>(CommonAttributes.Amount);
                    _inventoryButtonsText[i].Text = amount == int.MaxValue ? "∞" : amount.ToString();
                }
            }
            _renderer.UpdateView();
        }

        public void SetArray(InventoryArray New)
        {
            if(Array.Length != New.Length)
                throw new ArgumentOutOfRangeException($"New InventoryArray ({New.Length}) needs to be of the same size as the original.");
            _array = New;
        }

        public int Offset => _offset;
        public InventoryItemRenderer Renderer => _renderer;
        public InventoryArray Array => _array;
        public RenderableButton[] Buttons => _inventoryButtons;
        public RenderableText[] ButtonsText => _inventoryButtonsText;
        public BackgroundTexture[] Textures => _inventoryTextures;

        public virtual bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                if (this.Enabled) _panel.Enable();
                else _panel.Disable();
            }
        }
        public virtual Vector2 Scale
        {
            get => _scale;
            set
            {
                var elements = _panel.Elements.ToArray();
                for (var i = 0; i < elements.Length; i++)
                {
                    elements[i].Scale = 
                        new Vector2(elements[i].Scale.X / _individualScale.X, elements[i].Scale.Y / _individualScale.Y) * value;
                    var relativePosition = elements[i].Position - Position;
                    elements[i].Position = 
                        new Vector2(relativePosition.X / _scale.X, relativePosition.Y / _scale.Y) * value + Position;
                }
                _scale = value;
            }
        }

        public virtual Vector2 IndividualScale
        {
            get => _individualScale;
            set
            {
                var elements = _panel.Elements.ToArray();
                for (var i = 0; i < elements.Length; i++)
                {
                    elements[i].Scale = new Vector2(elements[i].Scale.X / _individualScale.X,
                                            elements[i].Scale.Y / _individualScale.Y) * value;
                }
                _individualScale = value;
            }
        }

        public virtual Vector2 Position
        {
            get => _position;
            set
            {
                var elements = _panel.Elements.ToArray();
                for (var i = 0; i < elements.Length; i++)
                {
                    elements[i].Position = elements[i].Position - _position + value;
                }
                _position = value;
            }
        }
    }
}
