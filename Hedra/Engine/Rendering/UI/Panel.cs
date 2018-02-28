/*
 * Author: Zaphyk
 * Date: 21/02/2016
 * Time: 12:54 a.m.
 *
 */
using System;
using System.Collections;
using System.Collections.Generic;
using OpenTK;
using Hedra.Engine.Management;
using Hedra.Engine.Events;
using OpenTK.Input;

namespace Hedra.Engine.Rendering.UI
{
	/// <summary>-
	/// Description of Panel.
	/// </summary>
	public delegate void OnPanelStateChangeEventHandler(object Sender, PanelState E);
	public delegate void OnEscapePressedEventHandler(object Sender, EventArgs E);
	
	public class Panel : EventListener
	{
		private const float MoveSpeed = 2f;
		public bool Animate = false;
		private List<UIElement> _elements = new List<UIElement>();
		public bool Enabled{get; private set;}
		public Vector2 Position{get; private set;}
		public event OnPanelStateChangeEventHandler OnPanelStateChange;
		public event OnEscapePressedEventHandler OnEscapePressed;

		public void Move(Vector2 Vec2){
			for(int i = 0; i < _elements.Count;i++){
				_elements[i].Position = _elements[i].Position + Vec2;
			}
			Position += Vec2;
		}
		
		public void MoveTo(Vector2 Vec2){
			for(int i = 0; i < _elements.Count;i++){
				Vector2 relativePos = _elements[i].Position - Position;
				_elements[i].Position = Vec2 + relativePos;
			}
			Position = Vec2;
		}

		public void Disable(){
			Enabled = false;
			for(int i = 0; i < _elements.Count;i++){
				_elements[i].Disable();
			}
			if(OnPanelStateChange != null)
				OnPanelStateChange.Invoke(this, PanelState.Disabled);
		}
		
		public void Enable(){
			Enabled = true;
			for(int i = 0; i < _elements.Count;i++){
				_elements[i].Enable();
			}
			if(OnPanelStateChange != null)
				OnPanelStateChange.Invoke(this, PanelState.Enabled);

		}

		public void AddElement(UIElement Element){
			if(!this.Enabled)
				Element.Disable();
			_elements.Add(Element);
		}
		
		public void RemoveElement(UIElement Element){
			_elements.Remove(Element);
			this.InitButtons();
		}
		
		private Button[][] _buttons;
		private void InitButtons(){
			
			//Now detect all the colums
			List<float> columns = new List<float>();
			for(int i = 0; i < _elements.Count; i++){
				if( _elements[i] is Button && (_elements[i] as Button).Enabled && !columns.Contains(_elements[i].Position.X) ) columns.Add( _elements[i].Position.X );
			}
			columns.Sort();
			
			_buttons = new Button[columns.Count][];
			for(int i = 0; i < _buttons.Length; i++){
				
				//First analyze the amount of rows
				List<float> rows = new List<float>();
				for(int l = 0; l < _elements.Count; l++){
					if( _elements[l] is Button && (_elements[l] as Button).Enabled && columns[i] == _elements[l].Position.X && !rows.Contains(_elements[l].Position.Y) ) rows.Add( _elements[l].Position.Y );
				}
				rows.Sort( new Comparison<float>( (F1, F2) => F2.CompareTo(F1) ));
				
				_buttons[i] = new Button[rows.Count];
				for(int j = 0; j < _buttons[i].Length; j++){
					for(int k = 0; k < _elements.Count; k++){
						if(_elements[k] is Button && (_elements[k] as Button).Enabled && columns[i] == _elements[k].Position.X && rows[j] == _elements[k].Position.Y)
							_buttons[i][j] = _elements[k] as Button;
					}
				}
			}
		}
		
		private bool _mFirstHover = false;
		private int _x = 0,_y = 0, _prevX, _prevY;//Indexes
		public override void OnKeyDown(object Sender, KeyboardKeyEventArgs E)
		{
			if(!Enabled) return;
			
			if(E.Key == Key.Escape){
				if(OnEscapePressed != null)
					OnEscapePressed.Invoke(this, E);
				return;
			}
			
			this.InitButtons();
			
			if(_buttons == null || _buttons.Length == 0 || _buttons[0].Length == 0) return;
			
			if(E.Key == Key.Enter && _mFirstHover){
				if(_buttons[_x][_y].Enabled)
					_buttons[_x][_y].OnHoverExit(Sender, E);
				_buttons[_x][_y].ForceClick();
				return;
			}
			
			if(E.Key == Key.Right) _x++;
			if(E.Key == Key.Left) _x--;
			if(E.Key == Key.Up) _y--;
			if(E.Key == Key.Down) _y++;
			
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
			
			
			if(_prevX != _x || _prevY != _y){
				if(_mFirstHover && _buttons[_prevX][_prevY].Enabled) _buttons[_prevX][_prevY].OnHoverExit(Sender, E);
				_mFirstHover = true;
				_prevX = _x;
				_prevY = _y;
				_buttons[_x][_y].OnHoverEnter(Sender, E);
			}
		}
		
		public override void OnMouseMove(object Sender, MouseMoveEventArgs E)
		{
			if(_mFirstHover && _buttons[_prevX][_prevY].Enabled) _buttons[_prevX][_prevY].OnHoverExit(Sender, E);
		}
		
	}
	
	public enum PanelState{
		Enabled,
		Disabled
	}
}
