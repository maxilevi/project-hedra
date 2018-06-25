/*
 * Author: Zaphyk
 * Date: 27/02/2016
 * Time: 05:30 a.m.
 *
 */
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;
using System.Drawing;
using Hedra.Engine.Rendering.UI;

namespace Hedra.Engine.EnvironmentSystem
{
	/// <summary>
	/// Description of Skydome.
	/// </summary>
	public sealed class Sky
	{
	    private static readonly Shader SkyGradientShader;
	    public bool Enabled { get; set; } = true;
        public Vector4 TopColor { get; set; } = Color.CornflowerBlue.ToVector4();
        public Vector4 BotColor { get; set; } = Color.LightYellow.ToVector4();
		private readonly Skydome _starsDome;
		private readonly Skydome _sunAndMoonDome;
		private int _previousShader;
		
        static Sky()
        {   
            SkyGradientShader = Shader.Build("Shaders/SkyGradient.vert", "Shaders/SkyGradient.frag");
        }

        public Sky()
        {
			_starsDome = new Skydome();
	        _sunAndMoonDome = new Skydome();
        }
		
		public void Draw(){
			if(!Enabled) return;

            GraphicsLayer.Disable(EnableCap.DepthTest);
			GraphicsLayer.Disable(EnableCap.Blend);
			_previousShader = GraphicsLayer.ShaderBound;

			SkyGradientShader.Bind();
		    SkyGradientShader["topColor"] = TopColor;
			SkyGradientShader["botColor"] = BotColor;
			SkyGradientShader["height"] = (float) GameSettings.Height;
		    DrawManager.UIRenderer.SetupQuad();
		    DrawManager.UIRenderer.DrawQuad();

			_starsDome.Draw();
			_sunAndMoonDome.Draw();

            GL.UseProgram(_previousShader);
			GraphicsLayer.ShaderBound = _previousShader;
            GraphicsLayer.Enable(EnableCap.DepthTest);
			GraphicsLayer.Enable(EnableCap.CullFace);
		}
	}
}