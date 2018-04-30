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

namespace Hedra.Engine.EnvironmentSystem
{
	/// <summary>
	/// Description of Sun.
	/// </summary>
	public class Sun
	{
	    public Vector3 Direction { get; set; }
        private static readonly Shader Shader;
		private readonly uint _textureId;
		private Vector3 _ndc;

	    static Sun()
	    {
	        Shader = Shader.Build("Shaders/Sun.vert", "Shaders/Sun.frag");
        }

		public Sun(Vector3 Direction){
			this.Direction = Direction;
			_textureId = Graphics2D.LoadFromAssets("Assets/Sun.png");
		}
		
		public Vector2 Coordinates => _ndc.Xy;

	    public bool Enabled => SkyManager.DayTime > 6000 && SkyManager.DayTime < 20000 && Math.Sin(GameManager.Player.View.TargetYaw) > 0;

	    public void Draw(){
			GraphicsLayer.Enable(EnableCap.Blend);
			GraphicsLayer.Disable(EnableCap.CullFace);
			GraphicsLayer.Disable(EnableCap.DepthTest);
			Shader.Bind(); 
			
			Matrix4 TransMatrix = Matrix4.CreateScale(new Vector3(0.073f, 0.13f, 1f) * 2 * 1.5f);//Magic numbers are the resolution used for development
			Vector4 EyeSpace = Vector4.Transform(new Vector4(ShaderManager.LightPosition,1), DrawManager.FrustumObject.ModelViewMatrix);
			Vector4 HomogeneusSpace = Vector4.Transform(EyeSpace, DrawManager.FrustumObject.ProjectionMatrix);
			_ndc = HomogeneusSpace.Xyz / HomogeneusSpace.W;

			if(Enabled){
				_ndc.Z = 0;
				_ndc.Y += 0.5f;
				Shader["Position"] = _ndc;
				Shader["TransMatrix"] = TransMatrix;
				Shader["Direction"] = Direction;
				
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, this._textureId);

			    DrawManager.UIRenderer.SetupQuad();
			    DrawManager.UIRenderer.DrawQuad();
            }
			
			Shader.UnBind();
			GraphicsLayer.Enable(EnableCap.CullFace);
			GraphicsLayer.Disable(EnableCap.Blend);
			GraphicsLayer.Enable(EnableCap.DepthTest);
		}
	}
}