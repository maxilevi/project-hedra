/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 21/06/2016
 * Time: 08:45 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.Events;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Scripting;
using Hedra.Game;
using OpenToolkit.Mathematics;
using OpenTK.Input;

namespace Hedra.Rendering.UI
{
    /// <summary>
    /// Description of TextField.
    /// </summary>
    public class TextField : Panel, UIElement, IUpdatable
    {
        private static readonly Script Script = Interpreter.GetScript("TextField.py");
        private readonly Dictionary<string, object> _state;
        private readonly Bar _textBar;
        private readonly RenderableText _caret;
        private readonly Button _focusButton;
        private int _caretIndex;

        public TextField(Vector2 Position, Vector2 Scale, bool CurvedBorders = true) : base()
        {
            _textBar = new Bar(Position, Scale, () => 1, () => 1, Vector4.One, this, DrawOrder.After, CurvedBorders)
            {
                ShowBar = false,
                UpdateTextRatio = false,
                AlignLeft = true
            };
            _focusButton = new Button(Position, Scale, GUIRenderer.TransparentTexture);
            _caret = new RenderableText(string.Empty, Vector2.Zero, _textBar.TextColor, _textBar.TextFont);
            _state = new Dictionary<string, object>();
            Script.Execute("init", _state, _textBar, _caret, _focusButton);
            UpdateManager.Add(this);
            DrawManager.UIRenderer.Add(_caret, DrawOrder.After);
            
            EventDispatcher.RegisterKeyDown(this, this.OnKeyDown, EventPriority.High);
            EventDispatcher.RegisterKeyPress(this, this.OnKeyPress);
            
            AddElement(_caret);
            AddElement(_focusButton);
            AddElement(_textBar);
        }
        
        public void Update()
        {
            if(!Enabled) return;
            Script.Execute("update_caret", _state);
        }
        
        private void OnKeyDown(object Sender, KeyEventArgs EventArgs)
        {
           Script.Execute("on_key_down", EventArgs, _state);
        }
        private void OnKeyPress(object Sender, KeyPressEventArgs E)
        {
            Script.Execute("on_key_press", E, _state);
        }

        public void Focus()
        {
            Script.Execute("focus", _state);
        }

        public void Defocus()
        {
            Script.Execute("defocus", _state);
        }
        
        public string Text
        {
            get => this._textBar.Text;
            set => Script.Execute("set_text", value, _state);
        }
        
        public Vector2 Scale
        {
            get => _textBar.Scale;
            set => _textBar.Scale = value;
        }
        
        public Vector2 Position
        {
            get => _textBar.Position;
            set => _textBar.Position = value;
        }

        public Font TextFont
        {
            get => _textBar.TextFont;
            set => _textBar.TextFont = value;
        }


        public override void Dispose()
        {
            base.Dispose();
            EventDispatcher.UnregisterKeyDown(this);
            EventDispatcher.UnregisterKeyPress(this);
            DrawManager.UIRenderer.Remove(_caret);
            UpdateManager.Remove(this);
            _textBar?.Dispose();
            _caret?.Dispose();
        }
    }
}
