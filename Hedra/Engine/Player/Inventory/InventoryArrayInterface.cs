﻿using System;
using System.Drawing;
using Hedra.Engine.Events;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Player.Inventory
{
    public class InventoryArrayInterface
    {
        private readonly InventoryArray _array;
        private readonly InventoryItemRenderer _renderer;
        private readonly Texture[] _inventoryTextures;
        private readonly RenderableButton[] _inventoryButtons;
        private readonly RenderableText[] _inventoryButtonsText;
        private readonly Panel _elementsPanel;
        private readonly int _length;
        private readonly int _offset;
        private Vector2 _position = Vector2.Zero;
        private Vector2 _individualScale = Vector2.One;
        private Vector2 _scale = Vector2.One;
        private bool _enabled;

        public InventoryArrayInterface(InventoryArray Array, int Offset, int Length, int SlotsPerLine, string[] CustomIcons = null)
        {
            this._array = Array;
            this._length = Length;
            this._offset = Offset;
            this._renderer = new InventoryItemRenderer(_array, _offset, _length);
            this._inventoryTextures = new Texture[_length];
            this._inventoryButtons = new RenderableButton[_length];
            this._inventoryButtonsText = new RenderableText[_length];
            this._elementsPanel = new Panel();
            var size = Graphics2D.SizeFromAssets("Assets/UI/InventorySlot.png");
            var offset = new Vector2(size.X, size.Y);
            var wholeSize = new Vector2(
                size.X * (_length - 1 - (_length - 1) / SlotsPerLine * SlotsPerLine),
                size.Y * ((_length - 1) / SlotsPerLine)
            );
            for (var i = 0; i < _length; i++)
            {
                var k = i;
                var j = i / SlotsPerLine;
                var scale = Vector2.One * .5f;
                var position = Vector2.Zero + new Vector2((i - j * SlotsPerLine) * offset.X, j * offset.Y) -
                               wholeSize * .5f;

                _inventoryTextures[i] = new Texture(CustomIcons != null ? CustomIcons[i] : "Assets/UI/InventorySlot.png", position, scale);
                _inventoryButtonsText[i] = new RenderableText(string.Empty, position + new Vector2(size.X, -size.Y) * .25f, Color.White, FontCache.Get(AssetManager.Fonts.Families[0], 10, FontStyle.Bold));
                _inventoryButtons[i] = new RenderableButton(position, size * scale * .8f, GUIRenderer.TransparentTexture);
                _inventoryButtons[i].Texture.IdPointer = () => _renderer.Draw(k);
                _inventoryButtons[i].PlaySound = false;
                _elementsPanel.AddElement(_inventoryTextures[i]);
                _elementsPanel.AddElement(_inventoryButtonsText[i]);
                _elementsPanel.AddElement(_inventoryButtons[i]);

                DrawManager.UIRenderer.Add(_inventoryButtons[i], DrawOrder.After);
                DrawManager.UIRenderer.Add(_inventoryButtonsText[i], DrawOrder.After);
            }
        }

        public void UpdateView()
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

        public int Offset => _offset;
        public InventoryItemRenderer Renderer => _renderer;
        public InventoryArray Array => _array;
        public RenderableButton[] Buttons => _inventoryButtons;

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                if(_enabled) _elementsPanel.Enable();
                else _elementsPanel.Disable();
            }
        }

        public Vector2 Scale
        {
            get { return _scale; }
            set
            {
                for (var i = 0; i < _inventoryTextures.Length; i++)
                {
                    _inventoryTextures[i].Scale = new Vector2(_inventoryTextures[i].Scale.X / _individualScale.X,
                                                      _inventoryTextures[i].Scale.Y / _individualScale.Y) * value;
                    _inventoryButtons[i].Scale = new Vector2(_inventoryButtons[i].Scale.X / _individualScale.X,
                                                     _inventoryButtons[i].Scale.Y / _individualScale.Y) * value;

                    var relativePosition = _inventoryTextures[i].Position - Position;
                    _inventoryTextures[i].Position = new Vector2(relativePosition.X / _scale.X,
                        relativePosition.Y / _scale.Y) * value + Position;

                    relativePosition = _inventoryButtons[i].Position - Position;
                    _inventoryButtons[i].Position = new Vector2(relativePosition.X / _scale.X,
                                                        relativePosition.Y / _scale.Y) * value + Position;

                    relativePosition = _inventoryButtonsText[i].Position - Position;
                    _inventoryButtonsText[i].Position = new Vector2(relativePosition.X / _scale.X,
                                                        relativePosition.Y / _scale.Y) * value + Position;
                }
                _scale = value;
            }
        }

        public Vector2 IndividualScale
        {
            get { return _individualScale; }
            set
            {
                for (var i = 0; i < _inventoryTextures.Length; i++)
                {
                    _inventoryTextures[i].Scale = new Vector2(_inventoryTextures[i].Scale.X / _individualScale.X,
                        _inventoryTextures[i].Scale.Y / _individualScale.Y) * value;
                    _inventoryButtons[i].Scale = new Vector2(_inventoryButtons[i].Scale.X / _individualScale.X,
                                                     _inventoryButtons[i].Scale.Y / _individualScale.Y) * value;
                }
                _individualScale = value;
            }
        }

        public Vector2 Position
        {
            get { return _position; }
            set
            {
                for (var i = 0; i < _inventoryTextures.Length; i++)
                {
                    _inventoryTextures[i].Position = _inventoryTextures[i].Position - _position + value;
                    _inventoryButtons[i].Position = _inventoryButtons[i].Position - _position + value;
                    _inventoryButtonsText[i].Position = _inventoryButtonsText[i].Position - _position + value;
                }
                _position = value;
            }
        }
    }
}