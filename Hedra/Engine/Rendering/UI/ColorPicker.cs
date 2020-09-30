/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 25/06/2016
 * Time: 10:30 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using Hedra.Engine.Management;
using System.Drawing;
using System.IO;
using Hedra.Core;
using System.Numerics;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.Rendering.UI;

namespace Hedra.Engine.Rendering.UI
{
    /// <summary>
    /// Description of ColorPicker.
    /// </summary>
    public delegate void ColorPickedEventHandler(Vector4 Color);
    
    public class ColorPicker : UIElement
    {
        public event ColorPickedEventHandler ColorPickedEvent;
        private readonly List<UIElement> _elements;
        private Vector2 _mScale;
        private Vector2 _mPosition;

        public ColorPicker(Vector4[] Colors, Translation NameTranslation, Vector2 Position, Vector2 Scale, Panel InPanel, int ColorsPerRow = 3)
        {
            _elements = new List<UIElement>();
            var rowCount = 0;
            var offset = Vector2.Zero;
            var realScale = Scale * 0.115f;
            var offsetStepX = Vector2.Zero;
            var offsetStepY = Vector2.Zero; 

            for(var i = 0; i < Colors.Length; i++)
            {
                var k = i;
                var backgroundTex = new BackgroundTexture("Assets/Background.png", Position + offset, realScale);
                var colorTex = new BackgroundTexture(Colors[i], Position + offset, backgroundTex.Scale * 0.85f);
                var btn = new Button(Position + offset, backgroundTex.Scale, GUIRenderer.TransparentTexture);
                btn.Click += (_, __) => ColorPickedEvent?.Invoke(Colors[k]);
                
                InPanel.AddElement(colorTex);
                InPanel.AddElement(backgroundTex);
                InPanel.AddElement(btn);
                _elements.Add(colorTex);
                _elements.Add(btn);
                _elements.Add(backgroundTex);
                
                rowCount++;
                offsetStepX = new Vector2(backgroundTex.Scale.X, 0);
                offsetStepY = new Vector2(0, backgroundTex.Scale.Y);
                
                offset += offsetStepX * 3;
                if (rowCount == ColorsPerRow)
                {
                    offset = new Vector2(0, offset.Y - offsetStepY.Y * 3);
                    rowCount = 0;
                }
            }
            
            var title = new GUIText(NameTranslation, Position + offsetStepX * ColorsPerRow  + offsetStepX * (ColorsPerRow-1) + offsetStepY * 2f, Color.White, FontCache.GetBold(22 * (Scale.X + Scale.Y) / 2f));
            title.Position -= title.Scale.X * Vector2.UnitX;
            _elements.Add(title);
            InPanel.AddElement(title);
        }

        public void Enable()
        {
            for(int i = 0; i < _elements.Count; i++)
            {
                _elements[i].Enable();
            }
        }
        
        public void Disable()
        {
            for(int i = 0; i < _elements.Count; i++)
            {
                _elements[i].Disable();
            }
        }
        
        public Vector2 Scale{
            get => _mScale;
            set{
                _mScale = value;
                for(var i = 0; i < _elements.Count; i++)
                {
                    _elements[i].Scale = value;
                }
            }
        }
        
        public Vector2 Position
        {
            get => _mPosition;
            set{
                for(var i = 0; i < _elements.Count; i++)
                {
                    _elements[i].Position = _elements[i].Position + value - _mPosition;
                }
                _mPosition = value;
            }
        }

        public void Dispose()
        {
            _elements.ForEach(D => D.Dispose());
        }    
    }
}
