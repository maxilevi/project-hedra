/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 21/06/2016
 * Time: 08:45 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using Hedra.Engine.Events;
using Hedra.Engine.Management;
using OpenTK;
using OpenTK.Input;
using System.Threading;
using System.Collections;

namespace Hedra.Engine.Rendering.UI
{
	/// <summary>
	/// Description of TextField.
	/// </summary>
	public class TextField : EventListener, UIElement
	{
		private Bar _textBar;
		private RenderableText _caret;
		private int _caretIndex;
		public bool UseNumbersOnly {get; set;}
		public bool ShowCaret {get; set;}
		
		public TextField(Vector2 Position, Vector2 Scale, Panel InPanel, bool CurveBorders = true){
			_textBar = new Bar(Position, Scale, () => 1, () => 1, Vector4.One, InPanel, DrawOrder.After, CurveBorders);
			_textBar.ShowBar = false;
			this.Text = string.Empty;
			_textBar.UpdateTextRatio = false;
			ShowCaret = false;
			_caret = new RenderableText("|", Position + Vector2.UnitY * .00275f, _textBar.Text.UIText.TextColor, _textBar.Text.UIText.TextFont);
			DrawManager.UIRenderer.Add(_caret, DrawOrder.After);
			InPanel.AddElement(_caret);
			CoroutineManager.StartCoroutine(this.CaretUpdate);
		}
		
		private IEnumerator CaretUpdate(){
			while(Program.GameWindow.Exists){
				
				for(int i = 0; i < 20; i++){
					if(!InFocus)_caret.Disable();
					yield return null;
				}
				//if(InFocus)
					_caret.Disable();
				
				for(int i = 0; i < 20; i++){
					if(!InFocus)_caret.Disable();
					yield return null;
				}
				if(InFocus && ShowCaret)
					_caret.Enable();
			}
		}
		
		public override void OnKeyDown(object Sender, KeyboardKeyEventArgs EventArgs){
			if(InFocus && this._enabled){
				if(EventArgs.Key == Key.BackSpace || EventArgs.Key == Key.BackSlash || EventArgs.Key == Key.NonUSBackSlash){
					if(Text.Length > 0){
						Text = Text.Substring(0, Text.Length-1);
						_caretIndex--;
						this.UpdateCaret();
					}else
						return;
					
					return;
				}/*
				if(e.Key == Key.Left){
					if(CaretIndex > 0)
						CaretIndex--;
					this.UpdateCaret();
				}
				if(e.Key == Key.Right){
					if(CaretIndex < Text.Length-1)
						CaretIndex++;
					this.UpdateCaret();
				}*/
			}
		}
		public override void OnKeyPress(object Sender, KeyPressEventArgs E){

			if(InFocus && this._enabled){
				
				float textSize = Graphics2D.LineSize(Text, _textBar.Text.UIText.TextFont).X/GameSettings.Width;
				if(textSize > _textBar.Scale.X)
					return;
				
				if(UseNumbersOnly && !char.IsNumber(E.KeyChar))return;
				
				if(ShowCaret){
					if(_caretIndex == Math.Max(0, Text.Length-1)){
						_caretIndex++;
						Text += E.KeyChar;
					}else{
						Text += " ";//placeholder
						char[] chars = Text.ToCharArray();
						for(int i = _caretIndex+1; i < Text.Length; i++){
							chars[i] = Text[i-1];
						}
						_caretIndex = (int) Mathf.Clamp(_caretIndex,0, Text.Length-1 );
						chars[_caretIndex] = E.KeyChar;
						Text = new string(chars);
					}
				}else{
					Text += E.KeyChar;
				}
				this.UpdateCaret();
				
			}
		}
		
		public void UpdateCaret(){
			_textBar.Text.UIText.Position = new Vector2(_textBar.Position.X - _textBar.Scale.X * .25f + _textBar.Text.UIText.Scale.X, _textBar.Text.UIText.Position.Y);
			if(ShowCaret){
				_caretIndex = (int) Mathf.Clamp(_caretIndex,0, Text.Length-1 );
				string beforeCaret = Text.Substring(0, (int)Mathf.Clamp(_caretIndex+1, 0, Text.Length));
				float sizeX = Graphics2D.LineSize(beforeCaret, _textBar.Text.UIText.TextFont).X*GameSettings.Width/GameSettings.Width * 2f;
				_caret.Position = new Vector2(_textBar.Position.X - _textBar.Scale.X + sizeX - 0.0025f, _caret.Position.Y);
			}
		}
		
		public override void OnMouseButtonDown(object Sender, MouseButtonEventArgs E){
			if(!_enabled)
				return;
			
			Vector2 coords = Mathf.ToNormalizedDeviceCoordinates(E.Mouse.X, E.Mouse.Y);
				
			if(Position.Y + Scale.Y > -coords.Y && Position.Y - Scale.Y < -coords.Y 
				  && Position.X + Scale.X > coords.X && Position.X - Scale.X < coords.X ){
				InFocus = true;
			}else{
				InFocus = false;
			}
		}
		
		private bool _mInFocus;
		public bool InFocus{
			get{ return _mInFocus; }
			set{
				_mInFocus = value;
				if(_mInFocus){
					this._textBar.BackgroundColor = new Vector4(0.0784313725f,0.0784313725f,0.0784313725f,1);
					if(ShowCaret){
						_caret.Enable();
						_caretIndex = this.Text.Length-1;
						this.UpdateCaret();
					}
				}else{
					this._textBar.BackgroundColor = new Vector4(0.1529f,0.1529f,0.1529f,1);
					if(ShowCaret){
						_caret.Disable();
						_caretIndex = 0;
						this.UpdateCaret();
					}
				}
			}
		}
		
		public string Text{
			get{ return this._textBar.Text.Text; }
			set{ this._textBar.Text.Text = value;}
		}
		
		public Vector2 Scale{
			get{ return _textBar.Scale; }
			set{ _textBar.Scale = _textBar.Scale; }
		}
		
		public Vector2 Position{
			get{ return _textBar.Position; }
			set{ _textBar.Position = _textBar.Position; }
		}
		
		private bool _enabled;
		public void Disable(){
			this._textBar.Disable();
			_enabled = false;
		}
		public void Enable(){
			this._textBar.Enable();
			_enabled = true;
		}
		
	}
}
