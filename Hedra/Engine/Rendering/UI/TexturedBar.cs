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
	    private static readonly Shader Shader;
		public Vector2 Scale {get; set;}
	    public bool ShowBar { get; set; } = true;
        private readonly Func<float> _value;
		private readonly Func<float> _max;
	    private readonly int _textureId;
        private bool _enabled;
		public Vector4 Color;
		private float _barSize;
	    private Vector2 _position;

	    static TexturedBar()
	    {
	        Shader = Shader.Build("Shaders/Bar.vert", "Shaders/Bar.frag");
        }

		public TexturedBar(uint TextureId, Vector2 Position, Vector2 Scale, Func<float> Value, Func<float> Max, Panel InPanel){
		    this.Scale = Scale;
		    this._value = Value;
		    this._max = Max;
		    this._textureId = (int)TextureId;

		    DrawManager.UIRenderer.Add(this, DrawOrder.After);

		    this.Position = Position;
        }
		
		public void Draw(){
			if(!_enabled)
				return;
			_barSize = Mathf.Clamp( Mathf.Lerp(_barSize, _value() / _max(), (float) Time.deltaTime * 8f), 0, 1);

			Shader.Bind();
			GraphicsLayer.Disable(EnableCap.CullFace);
			GraphicsLayer.Disable(EnableCap.DepthTest);
			GraphicsLayer.Enable(EnableCap.Blend);

		    GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _textureId);
			
			Shader["Scale"] = new Vector2(_barSize * Scale.X, Scale.Y);
			Shader["Position"] = Position - new Vector2(Scale.X * (1-_barSize), 0f);
			Shader["Color"] = -Vector4.One;

			DrawManager.UIRenderer.SetupQuad();
			DrawManager.UIRenderer.DrawQuad();	
			
			GraphicsLayer.Disable(EnableCap.Blend);
			GraphicsLayer.Enable(EnableCap.DepthTest);
			GraphicsLayer.Enable(EnableCap.CullFace);
			Shader.Unbind();
		}
		
		public Vector2 Position{
			get{ return _position;}
			set{
				_position = value;
			}
		}
		
		public void Enable(){
			this._enabled = true;
		}
		
		public void Disable(){
			this._enabled = false;
		}
		
		public void Dispose(){
			DrawManager.UIRenderer.Remove(this);
		}
	}
}
