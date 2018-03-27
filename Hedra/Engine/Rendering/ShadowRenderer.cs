/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 17/12/2016
 * Time: 07:06 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using OpenTK.Graphics.OpenGL;
using System;
using Hedra.Engine.Rendering;
using OpenTK;
using Hedra.Engine.Management;
using Hedra.Engine.Enviroment;
using Hedra.Engine.Player;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of ShadowRenderer.
	/// </summary>
	public static class ShadowRenderer
	{
		public static Vector3 LightPosition = new Vector3(-500,700,0);
		public static ShadowShader Shader = new ShadowShader("Shaders/Shadows.vert","Shaders/Shadows.frag");
		public static FBO ShadowFBO;
		public static Matrix4 DepthProj;
		public static Matrix4 DepthView;
		public static Matrix4 ShadowMVP;

	    private static float _shadowDistance;
	    public static float ShadowDistance
	    {
	        get { return _shadowDistance; }
	        set
	        {
	            _shadowDistance = value;
	            DepthProj = Matrix4.CreateOrthographicOffCenter(-_shadowDistance, _shadowDistance,
	                -_shadowDistance, _shadowDistance,
	                -_shadowDistance, _shadowDistance);
	        }
	    }
	    private static int PrevFBO;

	    public static void Bind()
	    {
	        ShadowDistance = 1400f / (float) GameSettings.MaxLoadingRadius * GameSettings.ChunkLoaderRadius;
            PrevFBO = GraphicsLayer.FBOBound;
	        if (ShadowFBO == null)
	            ShadowRenderer.SetQuality(GameSettings.ShadowQuality);
	        ShadowFBO.Bind();

	        //ShaderManager.LightPosition = Vector3.TransformNormal(LightPosition.NormalizedFast(), Matrix4.CreateRotationY(SkyManager.SkyModifier * 360 * Mathf.RADIAN));
	        Vector3 Position = Scenes.SceneManager.Game.Player.View.Position +
	                           Scenes.SceneManager.Game.Player.View.LookAtPoint;
	        Vector3 NormalizedLight =
	            (new Vector3(LightPosition.X, LightPosition.Y, LightPosition.Z))
	            .NormalizedFast(); //ShaderManager.LightPosition
	        NormalizedLight = Vector3.TransformNormal(new Vector3(NormalizedLight.X, NormalizedLight.Y, NormalizedLight.Z),
	            Matrix4.CreateRotationY( SkyManager.DayTime / 24000 * 360f * Mathf.Radian));

	    Matrix4 NewModel = Matrix4.Identity;
            ShadowMVP = NewModel * Matrix4.LookAt( NormalizedLight + Position, Position, Vector3.UnitY) * DepthProj;

			Shader.Bind();
			GL.Uniform1(Shader.TimeUniform, Time.CurrentFrame );
			GL.Uniform1(Shader.FancyUniform, (GameSettings.Fancy) ? 1.0f : 0.0f);
			GL.UniformMatrix4(Shader.MVPUniform, false, ref ShadowMVP);
			GL.CullFace(CullFaceMode.Front);
            GL.Enable(EnableCap.CullFace);
		}
		
		public static void UnBind()
		{
		    DrawManager.FrustumObject.SetViewport();
            Shader.UnBind();
			GL.CullFace(CullFaceMode.Back);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, PrevFBO);
			GraphicsLayer.FBOBound = PrevFBO;
		}
		
		public static void SetQuality(int Quality){

		    ShadowFBO?.Dispose();

		    if(Quality == 2 || Quality == 1)
			 	ShadowFBO = new FBO(2048, 2048, false, 0, FramebufferAttachment.DepthAttachment, PixelInternalFormat.DepthComponent16, false, false);

		    if (Quality == 3)
		        ShadowFBO = new FBO(4096, 4096, false, 0, FramebufferAttachment.DepthAttachment, PixelInternalFormat.DepthComponent16, false, false);
        }
	}
}
