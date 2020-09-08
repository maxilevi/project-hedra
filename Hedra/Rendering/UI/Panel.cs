/*
 * Author: Zaphyk
 * Date: 21/02/2016
 * Time: 12:54 a.m.
 *
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Hedra.Engine.Events;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Windowing;
using System.Numerics;
using Silk.NET.Input.Common;
using Button = Hedra.Engine.Rendering.UI.Button;


namespace Hedra.Rendering.UI
{
    /// <summary>-
    /// Description of Panel.
    /// </summary>
    public delegate void OnPanelStateChangeEventHandler(object Sender, PanelState E);
    public delegate void OnEscapePressedEventHandler(object Sender, EventArgs E);
    
    public class Panel : EventListener, UIElement
    {
        private const float MoveSpeed = 2f;
        private Vector2 _position;
        public bool Animate = false;
        public bool DisableKeys { get; set; }
        public bool Enabled { get; private set; }
        public event OnPanelStateChangeEventHandler OnPanelStateChange;
        public event OnEscapePressedEventHandler OnEscapePressed;
        private bool _firstHover;
        private int _x;
        private int _y;
        private int _prevX;
        private int _prevY;
        private Button[][] _buttons;
        private readonly List<UIElement> _elements;
        public ReadOnlyCollection<UIElement> Elements => _elements.AsReadOnly();

        public Panel()
        {
            _elements = new List<UIElement>();
        }

        public void Move(Vector2 Vec2)
        {
            for(var i = 0; i < _elements.Count;i++)
            {
                _elements[i].Position = _elements[i].Position + Vec2;
            }
            Position += Vec2;
        }
        
        public void MoveTo(Vector2 Vec2)
        {
            for(var i = 0; i < _elements.Count;i++)
            {
                Vector2 relativePos = _elements[i].Position - Position;
                _elements[i].Position = Vec2 + relativePos;
            }
            Position = Vec2;
        }

        public void Disable()
        {
            Enabled = false;
            for(var i = 0; i < _elements.Count;i++)
            {
                _elements[i].Disable();
            }
            OnPanelStateChange?.Invoke(this, PanelState.Disabled);
        }
        
        public void Enable()
        {
            Enabled = true;
            for(int i = 0; i < _elements.Count;i++)
            {
                _elements[i].Enable();
            }
            OnPanelStateChange?.Invoke(this, PanelState.Enabled);
        }

        public void AddElement(UIElement Element)
        {
            if(!this.Enabled)
                Element.Disable();
            _elements.Add(Element);
        }
        
        public void RemoveElement(UIElement Element)
        {
            _elements.Remove(Element);
            this.InitializeButtons();
        }
        
        private void InitializeButtons()
        {        
            var columns = new List<float>();
            for(var i = 0; i < _elements.Count; i++)
            {
                if( _elements[i] is Button && (_elements[i] as Button).Enabled && !columns.Contains(_elements[i].Position.X) ) columns.Add( _elements[i].Position.X );
            }
            columns.Sort();
            
            _buttons = new Button[columns.Count][];
            for(var i = 0; i < _buttons.Length; i++)
            {
                
                var rows = new List<float>();
                for(int l = 0; l < _elements.Count; l++)
                {
                    if( _elements[l] is Button && (_elements[l] as Button).Enabled && columns[i] == _elements[l].Position.X && !rows.Contains(_elements[l].Position.Y) ) rows.Add( _elements[l].Position.Y );
                }
                rows.Sort( new Comparison<float>( (F1, F2) => F2.CompareTo(F1) ));
                
                _buttons[i] = new Button[rows.Count];
                for(var j = 0; j < _buttons[i].Length; j++)
                {
                    for(var k = 0; k < _elements.Count; k++)
                    {
                        if(_elements[k] is Button && (_elements[k] as Button).Enabled && columns[i] == _elements[k].Position.X && rows[j] == _elements[k].Position.Y)
                            _buttons[i][j] = _elements[k] as Button;
                    }
                }
            }
        }

        public void ResetSelected()
        {
            _firstHover = false;
            _x = 0;
            _y = 0;
            _prevX = 0;
            _prevY = 0;
        }

        public override void OnKeyDown(object Sender, KeyEventArgs EventArgs)
        {
            if(!Enabled || DisableKeys) return;
            
            if(EventArgs.Key == Key.Escape)
            {
                OnEscapePressed?.Invoke(this, EventArgs);
                return;
            }
            
            this.InitializeButtons();
            
            if(_buttons == null || _buttons.Length == 0 || _buttons[0].Length == 0) return;
            
            if(EventArgs.Key == Key.Enter && _firstHover)
            {
                if(_buttons[_x][_y].Enabled)
                    _buttons[_x][_y].OnHoverExit();
                _buttons[_x][_y].ForceClick();
                return;
            }
            
            if(EventArgs.Key == Key.Right) _x++;
            if(EventArgs.Key == Key.Left) _x--;
            if(EventArgs.Key == Key.Up) _y--;
            if(EventArgs.Key == Key.Down) _y++;
            
            if(_y == -1 && _buttons[_x].Length-1 == 0){
                int newX = _x;
                for(int i = 0; i < _buttons.Length; i++){
                    if(_buttons[i].Length-1 > 0){
                        newX = i;
                    }
                }
                _x = newX;
            }
            
            if(_x < 0) _x = _buttons.Length-1;
            if(_x > _buttons.Length-1) _x = 0;
            
            if(_y< 0) _y = _buttons[_x].Length-1;
            if(_y > _buttons[_x].Length-1) _y = 0;


            if (_prevX == _x && _prevY == _y) return;
            if(_firstHover && _buttons[_prevX][_prevY].Enabled) _buttons[_prevX][_prevY].OnHoverExit();
            _firstHover = true;
            _prevX = _x;
            _prevY = _y;
            _buttons[_x][_y].OnHoverEnter();
        }
        
        public override void OnMouseMove(object Sender, MouseMoveEventArgs E)
        {
            if(_firstHover && _buttons[_prevX][_prevY].Enabled) _buttons[_prevX][_prevY].OnHoverExit();
        }
        
        public virtual Vector2 Position
        {
            get => _position;
            set
            {
                var elements = Elements.ToArray();
                for (var i = 0; i < elements.Length; i++)
                {
                    elements[i].Position = elements[i].Position - _position + value;
                }
                _position = value;
            }
        }
        
        public virtual Vector2 Scale
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
    }
    
    public enum PanelState{
        Enabled,
        Disabled
    }
}
