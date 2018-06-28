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
	internal class DistortionFilter : Filter
	{
	    private static readonly Shader WaterEffect;
	    private static readonly uint DuDvMapId;
        private int _duDvMapUniform;
	    private int _timeUniform;

	    static DistortionFilter()
	    {
	        WaterEffect = Shader.Build("Shaders/UnderWater.vert", "Shaders/Distortion.frag");
	        DuDvMapId = Graphics2D.LoadFromAssets("Assets/DuDvMap.png");
        }

		public DistortionFilter() : base(){
			_duDvMapUniform = GL.GetUniformLocation(WaterEffect.ShaderId, "DuDvMap");
			_timeUniform = GL.GetUniformLocation(WaterEffect.ShaderId, "Time");
		}
		
		public override void Resize(){}
		
		public override void Pass(FBO Src, FBO Dst){
			Dst.Bind();
			
			WaterEffect.Bind();
			//GL.Uniform1(TimeUniform, WaterMeshBuffer.WaveMovement);
			
			GL.ActiveTexture(TextureUnit.Texture1);
			GL.BindTexture(TextureTarget.Texture2D, DuDvMapId);
			//GL.Uniform1(DuDvMapUniform, 1);
			
			DrawQuad(WaterEffect, Src.TextureID[0]);
			WaterEffect.Unbind();
			
			Dst.UnBind();
		}
	}
}
