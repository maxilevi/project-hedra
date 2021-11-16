using System;
using System.Linq;
using System.Numerics;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.UI;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using SixLabors.ImageSharp;

namespace Hedra.Engine.Player.Inventory
{
    public class InventoryArrayInterface
    {
        public const string DefaultIcon = "Assets/UI/InventorySlot.png";
        public const float UISizeMultiplier = 1.15f;
        private readonly int _length;
        protected readonly Panel _panel;

        private bool _enabled;
        private Vector2 _individualScale = Vector2.One;
        private Vector2 _position = Vector2.Zero;
        private Vector2 _scale = Vector2.One;

        public InventoryArrayInterface(InventoryArray Array, int Offset, int Length, int SlotsPerLine,
            Vector2 Spacing, string[] CustomIcons = null)
        {
            this.Array = Array;
            _length = Length;
            this.Offset = Offset;
            Renderer = new InventoryItemRenderer(this.Array, this.Offset, _length);
            Textures = new BackgroundTexture[_length];
            Buttons = new RenderableButton[_length];
            ButtonsText = new RenderableText[_length];
            _panel = new Panel { DisableKeys = true };
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
                var customId = CustomIcons != null
                    ? Graphics2D.LoadFromAssets(CustomIcons[i])
                    : GUIRenderer.TransparentTexture;
                var customScale = /*CustomIcons != null ? Graphics2D.SizeFromAssets(CustomIcons[i]) : */DefaultSize;

                Textures[i] = new BackgroundTexture(CustomIcons != null ? customId : DefaultId, position,
                    customScale * scale);
                ButtonsText[i] = new RenderableText(string.Empty,
                    position + new Vector2(size.X, -size.Y) * .25f, Color.White, FontCache.GetBold(10));
                Buttons[i] =
                    new RenderableButton(position, size * scale * .8f, GUIRenderer.TransparentTexture);
                Buttons[i].Texture.IdPointer = () => Renderer.Draw(k);
                Buttons[i].PlaySound = false;
                _panel.AddElement(Textures[i]);
                _panel.AddElement(ButtonsText[i]);
                _panel.AddElement(Buttons[i]);

                DrawManager.UIRenderer.Add(Buttons[i], DrawOrder.After);
                DrawManager.UIRenderer.Add(ButtonsText[i], DrawOrder.After);
            }
        }

        public static uint DefaultId { get; } = Graphics2D.LoadFromAssets(DefaultIcon);

        public static Vector2 DefaultSize { get; } =
            Graphics2D.SizeFromAssets(DefaultIcon).As1920x1080() * UISizeMultiplier;

        public int Offset { get; }

        public InventoryItemRenderer Renderer { get; }

        public InventoryArray Array { get; private set; }

        public RenderableButton[] Buttons { get; }

        public RenderableText[] ButtonsText { get; }

        public BackgroundTexture[] Textures { get; }

        public virtual bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                if (Enabled) _panel.Enable();
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
                        new Vector2(elements[i].Scale.X / _individualScale.X,
                            elements[i].Scale.Y / _individualScale.Y) * value;
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
                    elements[i].Scale = new Vector2(elements[i].Scale.X / _individualScale.X,
                        elements[i].Scale.Y / _individualScale.Y) * value;
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
                    elements[i].Position = elements[i].Position - _position + value;
                _position = value;
            }
        }

        public virtual void UpdateView()
        {
            for (var i = 0; i < _length; i++)
                if (Array[i] == null || !Array[i].HasAttribute(CommonAttributes.Amount))
                {
                    ButtonsText[i].Text = string.Empty;
                }
                else
                {
                    var amount = Array[i].GetAttribute<int>(CommonAttributes.Amount);
                    ButtonsText[i].Text = amount == int.MaxValue ? "∞" : amount.ToString();
                }

            Renderer.UpdateView();
        }

        public void SetArray(InventoryArray New)
        {
            if (Array.Length != New.Length)
                throw new ArgumentOutOfRangeException(
                    $"New InventoryArray ({New.Length}) needs to be of the same size as the original.");
            Array = New;
            Renderer.SetArray(New);
        }
    }
}