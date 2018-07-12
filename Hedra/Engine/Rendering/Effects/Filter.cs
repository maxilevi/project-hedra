/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/08/2016
 * Time: 01:16 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Management;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Hedra.Engine.Rendering.Effects
{
	/// <summary>
	/// Description of IFilter.
	/// </summary>
	internal abstract class Filter : IDisposable
	{	

		public abstract void Resize();
	    public abstract void Dispose();
        public abstract void Pass(FBO Src, FBO Dst);
		
		public virtual void DrawQuad(Shader DrawingShader, uint TexID, uint Additive = 0, bool Flipped = false){
			Renderer.Enable(EnableCap.Texture2D);
			Renderer.Disable(EnableCap.DepthTest);

		    DrawManager.UIRenderer.SetupQuad();

            GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, TexID);

            if (DrawingShader.HasUniform("Scale")) DrawingShader["Scale"] = Vector2.One;
		    if (DrawingShader.HasUniform("Position"))  DrawingShader["Position"] = Vector2.Zero;

		    DrawManager.UIRenderer.DrawQuad();

            Renderer.Enable(EnableCap.DepthTest);
			Renderer.Disable(EnableCap.Texture2D);
			Renderer.Enable(EnableCap.CullFace);
		}
	}
}
