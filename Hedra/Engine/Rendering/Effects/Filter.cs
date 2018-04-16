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
using Hedra.Engine.Rendering.UI;

namespace Hedra.Engine.Rendering.Effects
{
	/// <summary>
	/// Description of IFilter.
	/// </summary>
	public abstract class Filter
	{	

		public abstract void Resize();
		public abstract void Pass(FBO Src, FBO Dst);
		
		public virtual void DrawQuad(uint TexID, uint Additive = 0, bool Flipped = false){
			GL.Enable(EnableCap.Texture2D);
			GL.Disable(EnableCap.DepthTest);

		    DrawManager.UIRenderer.SetupQuad();

            GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, TexID);
			
			MainFBO.Shader["Scale"] = Vector2.One;
			MainFBO.Shader["Position"] = Vector3.Zero;

		    DrawManager.UIRenderer.DrawQuad();

            GL.Enable(EnableCap.DepthTest);
			GL.Disable(EnableCap.Texture2D);
			GL.Enable(EnableCap.CullFace);
		}
	}
}
