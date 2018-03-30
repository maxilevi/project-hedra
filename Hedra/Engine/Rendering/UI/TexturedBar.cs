/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 14/06/2016
 * Time: 07:54 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using System.IO;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering.UI
{
	
	public class TexturedBar : IRenderable, UIElement, IDisposable
	{
		public static BarShader Shader = new BarShader("Shaders/Bar.vert", "Shaders/Bar.frag");
		public Vector2 Scale {get; set;}
		
		private Func<float> _value;
		private Func<float> _max;
		private bool _enabled;
		public Vector4 Color;
		//public RenderableText Text;
		private Panel _inPanel;
		private float _barSize;
		//private bool m_ShowText = true;
		private Vector2 _targetResolution = new Vector2(1024,578);
		private int _textureId;
		/*
		public bool ShowText{
			get{ return m_ShowText; }
			set{
				m_ShowText = value;
				//if(value)
				//	Text.Enable();
				//else
				//	Text.Disable();
			}
		}*/
		public bool ShowBar = true;
		
		public TexturedBar(uint TextureId, Vector2 Position, Vector2 Scale, Func<float> Value, Func<float> Max, Panel InPanel){
			Initialize(TextureId, Position, Scale, Value, Max, InPanel);
		}
		
		private void Initialize(uint TextureId, Vector2 Position, Vector2 Scale, Func<float> Value, Func<float> Max, Panel InPanel){
			this.Scale = Scale;
			this._value = Value;
			this._max = Max;
			this._inPanel = InPanel;
			this._textureId = (int) TextureId;
			
			DrawManager.UIRenderer.Add(this, DrawOrder.After);
			//Text = new RenderableText(Value() + " / " + Max(), Position, System.Drawing.Color.White, FontCache.Get(AssetManager.Fonts.Families[0], 11, FontStyle.Bold));

			//DrawManager.UIRenderer.Add(Text, false);
			//InPanel.AddElement(Text);
			
			this.Position = Position;
		}
		
		public void Draw(){
			if(!_enabled)
				return;
			_barSize = Mathf.Clamp( Mathf.Lerp(_barSize, _value() / _max(), (float) Time.deltaTime * 8f), 0, 1);
			
			//Text.Text = (int) ((Value() / Max())*100f)+"%";
			Shader.Bind();
			GL.Disable(EnableCap.CullFace);
			GL.Disable(EnableCap.DepthTest);
			GL.Enable(EnableCap.Blend);

		    GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _textureId);
			
			GL.Uniform2(Shader.ScaleUniform, new Vector2(_barSize * Scale.X, Scale.Y) );//Mathf.DivideVector(TargetResolution * Scale, new Vector2(GameSettings.Width, GameSettings.Height)) + Mathf.DivideVector(TargetResolution * new Vector2(0.015f,0.015f), new Vector2(GameSettings.Width, GameSettings.Height)));
			GL.Uniform2(Shader.PositionUniform, Position - new Vector2(Scale.X * (1-_barSize), 0f));
			GL.Uniform4(Shader.ColorUniform, -Vector4.One);

			DrawManager.UIRenderer.SetupQuad();
			DrawManager.UIRenderer.DrawQuad();	
			
			GL.Disable(EnableCap.Blend);
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);
			Shader.UnBind();
		}
		
		private Vector2 _mPosition;
		public Vector2 Position{
			get{ return _mPosition;}
			set{
				_mPosition = value;
				//Text.Position = m_Position;
			}
		}
		
		public void Enable(){
			this._enabled = true;
			//Text.Enable();
		}
		
		public void Disable(){
			this._enabled = false;
			//Text.Disable();
		}
		
		public void Dispose(){
			//this.Text.Dispose();
			DrawManager.UIRenderer.Remove(this);
		}
	}
}
