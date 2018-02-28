/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/11/2016
 * Time: 09:08 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Rendering.UI;

namespace Hedra.Engine.Rendering.Effects
{
	/// <summary>
	/// Description of UnderWaterFilter.
	/// </summary>
	public class DistortionFilter : Filter
	{
		public GUIShader WaterEffect = new GUIShader("Shaders/UnderWater.vert", "Shaders/Distortion.frag");
		private int DuDvMapUniform, TimeUniform;
		private uint DuDvMapId = Graphics2D.LoadFromAssets("Assets/DuDvMap.png");
		
		public DistortionFilter() : base(){
			DuDvMapUniform = GL.GetUniformLocation(WaterEffect.ShaderID, "DuDvMap");
			TimeUniform = GL.GetUniformLocation(WaterEffect.ShaderID, "Time");
		}
		
		public override void Resize(){}
		
		public override void Pass(FBO Src, FBO Dst){
			Dst.Bind();
			
			WaterEffect.Bind();
			//GL.Uniform1(TimeUniform, WaterMeshBuffer.WaveMovement);
			
			GL.ActiveTexture(TextureUnit.Texture1);
			GL.BindTexture(TextureTarget.Texture2D, DuDvMapId);
			//GL.Uniform1(DuDvMapUniform, 1);
			
			DrawQuad(Src.TextureID[0]);
			WaterEffect.UnBind();
			
			Dst.UnBind();
		}
	}
}
