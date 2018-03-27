/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 03/12/2016
 * Time: 07:42 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.UI;

namespace Hedra.Engine.Enviroment
{
	/// <summary>
	/// Description of Sun.
	/// </summary>
	public class Sun
	{
		public Vector3 Direction;
		private SunShader Shader = new SunShader("Shaders/Sun.vert","Shaders/Sun.frag");
		private uint TextureID;
		private Vector3 NDC;
		
		public Sun(Vector3 Direction){
			this.Direction = Direction;
			TextureID = Graphics2D.LoadFromAssets("Assets/Sun.png");
		}
		
		public Vector2 Coordinates{
			get{ return NDC.Xy;}
		}
		
		public bool Enabled{
			get{ return SkyManager.DayTime > 6000 && SkyManager.DayTime < 20000 && Math.Sin(Scenes.SceneManager.Game.Player.View.Yaw) > 0; }
		}
		
		public void Draw(){
			GL.Enable(EnableCap.Blend);
			GL.Disable(EnableCap.CullFace);
			GL.Disable(EnableCap.DepthTest);
			Shader.Bind(); 
			
			Matrix4 TransMatrix = Matrix4.CreateScale(new Vector3(0.073f, 0.13f, 1f) * 2 * 1.5f);//Magic numbers are the resolution used for development
			Vector4 EyeSpace = Vector4.Transform(new Vector4(ShaderManager.LightPosition,1), DrawManager.FrustumObject.ModelViewMatrix);
			Vector4 HomogeneusSpace = Vector4.Transform(EyeSpace, DrawManager.FrustumObject.ProjectionMatrix);
			NDC = HomogeneusSpace.Xyz / HomogeneusSpace.W;

			if(Enabled){
				NDC.Z = 0;
				NDC.Y += 0.5f;
				GL.Uniform3(Shader.PositionUniform, NDC);
				GL.UniformMatrix4(Shader.TransMatrixUniform, false, ref TransMatrix);
				GL.Uniform3(Shader.DirectionUniform, Direction);
				
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, this.TextureID);

			    DrawManager.UIRenderer.SetupQuad();
			    DrawManager.UIRenderer.DrawQuad();
            }
			
			Shader.UnBind();
			GL.Enable(EnableCap.CullFace);
			GL.Disable(EnableCap.Blend);
			GL.Enable(EnableCap.DepthTest);
		}
	}
}