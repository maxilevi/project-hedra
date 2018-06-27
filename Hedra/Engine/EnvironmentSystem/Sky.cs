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
		private readonly SkyOverlay _starsDome;
		private readonly SkyOverlay _sunDome;
	    private readonly VBO<Vector3> _gradientDome;
        private int _previousShader;
		
        static Sky()
        {   
            SkyGradientShader = Shader.Build("Shaders/SkyGradient.vert", "Shaders/SkyGradient.frag");
        }

        public Sky()
        {
			_starsDome = new SkyOverlay(new []
			{
			    "Assets/Sky/stars_right.png",
			    "Assets/Sky/stars_left.png",
			    "Assets/Sky/stars_top.png",
			    "Assets/Sky/stars_bottom.png",
			    "Assets/Sky/stars_front.png",
			    "Assets/Sky/stars_back.png"
            });
	        _sunDome = new SkyOverlay(new[]
	        {
	            null,
	            null,
                "Assets/Sky/sunandmoon_top.png",
	            null,
	            null,
	            null
            });
        }
		
		public void Draw()
        {
			if(!Enabled) return;

            GraphicsLayer.Disable(EnableCap.DepthTest);
			GraphicsLayer.Disable(EnableCap.Blend);
			_previousShader = GraphicsLayer.ShaderBound;

			SkyGradientShader.Bind();
		    SkyGradientShader["topColor"] = TopColor;
			SkyGradientShader["botColor"] = BotColor;
			SkyGradientShader["height"] = (float) GameSettings.Height * (1 - GameManager.Player.View.Pitch * .25f);
		    DrawManager.UIRenderer.SetupQuad();
		    DrawManager.UIRenderer.DrawQuad();

            _starsDome.ColorMultiplier = Vector4.One * (1-SkyManager.LastDayFactor) * 2f;
			_starsDome.Draw();
		    _sunDome.TransformationMatrix = Matrix4.CreateRotationX(Mathf.Radian * (1-SkyManager.StackedDaytimeModifier) * 180f);
		    _sunDome.ColorMultiplier = Vector4.One * SkyManager.LastDayFactor * 2f;
            _sunDome.Draw();

            GL.UseProgram(_previousShader);
			GraphicsLayer.ShaderBound = _previousShader;
            GraphicsLayer.Enable(EnableCap.DepthTest);
			GraphicsLayer.Enable(EnableCap.CullFace);
		}
	}
}