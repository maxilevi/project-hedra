/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/08/2016
 * Time: 01:17 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Management;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Rendering.UI;

namespace Hedra.Engine.Rendering.Effects
{
	/// <summary>
	/// Description of UnderWaterFilter.
	/// </summary>
	public class UnderWaterFilter : Filter
	{
		public GUIShader WaterEffect = new GUIShader("Shaders/UnderWater.vert", "Shaders/UnderWater.frag");
		public static Vector4 Multiplier = new Vector4(1f,1f,1f ,1);
		private int MultiplierUniform, TimeUniform;
		
		public UnderWaterFilter() : base(){
			MultiplierUniform = GL.GetUniformLocation(WaterEffect.ShaderID, "Multiplier");
			TimeUniform = GL.GetUniformLocation(WaterEffect.ShaderID, "Time");
		}
		
		public override void Resize(){}
		
		public override void Pass(FBO Src, FBO Dst){
			Dst.Bind();
			
			WaterEffect.Bind();
			GL.Uniform1(TimeUniform, WaterMeshBuffer.WaveMovement);
			GL.Uniform4(MultiplierUniform, Multiplier);
			this.DrawQuad(Src.TextureID[0]);
			WaterEffect.UnBind();
			
			Dst.UnBind();
		}
		
		public override void DrawQuad(uint TexID, uint Additive = 0, bool Flipped = false){
			GL.Enable(EnableCap.Texture2D);
			GL.Disable(EnableCap.DepthTest);
			
			DrawManager.UIRenderer.SetupQuad();
			
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, TexID);

		    DrawManager.UIRenderer.DrawQuad();

            GL.Enable(EnableCap.DepthTest);
			GL.Disable(EnableCap.Texture2D);
			GL.Enable(EnableCap.CullFace);
		}
	}
}
