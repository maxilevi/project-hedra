/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 17/12/2016
 * Time: 07:06 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using OpenTK.Graphics.OpenGL4;
using System;
using Hedra.Core;
using Hedra.Engine.Rendering;
using OpenTK;
using Hedra.Engine.Management;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Game;
using Hedra.Engine.IO;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Frustum;

namespace Hedra.Engine.Rendering
{
    /// <summary>
    /// Description of ShadowRenderer.
    /// </summary>
    public static class ShadowRenderer
    {
        public static Vector3 LightPosition { get; set; }
        public static Shader Shader { get; set; }
        public static FBO ShadowFbo { get; set; }
        public static Matrix4 DepthProj { get; set; }
        public static Matrix4 DepthView { get; set; }
        public static Matrix4 ShadowMvp { get; set; }
        private static float _shadowDistance;
        private static uint _prevFbo;

        static ShadowRenderer()
        {
            LightPosition = new Vector3(-500, 700, 0);
            Shader = Shader.Build("Shaders/Shadows.vert", "Shaders/Shadows.frag");
            ShadowDistance = 1400;
        }

        public static float ShadowDistance
        {
            get => _shadowDistance;
            set
            {
                if (_shadowDistance == value) return;
                _shadowDistance = value;
                DepthProj = Matrix4.CreateOrthographicOffCenter(-_shadowDistance, _shadowDistance,
                    -_shadowDistance, _shadowDistance,
                    -_shadowDistance, _shadowDistance);
            }
        }        

        public static void Bind()
        {
            _prevFbo = Renderer.FBOBound;
            if (ShadowFbo == null) ShadowRenderer.SetQuality(GameSettings.ShadowQuality);
            ShadowFbo.Bind();

            //ShaderManager.LightPosition = Vector3.TransformNormal(LightPosition.NormalizedFast(), Matrix4.CreateRotationY(SkyManager.SkyModifier * 360 * Mathf.RADIAN));
            Vector3 Position = GameManager.Player.View.CameraEyePosition.Xz.ToVector3() + Vector3.UnitY * 800;
            Vector3 NormalizedLight =
                (new Vector3(LightPosition.X, LightPosition.Y, LightPosition.Z))
                .NormalizedFast(); //ShaderManager.LightPosition
            NormalizedLight = Vector3.TransformNormal(new Vector3(NormalizedLight.X, NormalizedLight.Y, NormalizedLight.Z),
                Matrix4.CreateRotationY( SkyManager.DayTime / 24000 * 360f * Mathf.Radian));

            var newModel = Matrix4.Identity;
            ShadowMvp = newModel * Matrix4.LookAt( NormalizedLight + Position, Position, Vector3.UnitY) * DepthProj;

            Shader.Bind();
            Shader["Time"] = Time.AccumulatedFrameTime;
            Shader["Fancy"] = GameSettings.Quality ? 1.0f : 0.0f;
            Shader["MVP"] = ShadowMvp;
            Renderer.CullFace(CullFaceMode.Front);
            Renderer.Enable(EnableCap.CullFace);
        }
        
        public static void UnBind()
        {
            Culling.SetViewport();
            Shader.Unbind();
            Renderer.CullFace(CullFaceMode.Back);
            Renderer.BindFramebuffer(FramebufferTarget.Framebuffer, _prevFbo);
        }
        
        public static void SetQuality(int Quality)
        {
            Log.WriteLine($"Setting shadow quality to {Quality}");
            ShadowFbo?.Dispose();
            var size = Vector2.Zero;
            switch (Quality)
            {
                case 1:
                    size = new Vector2(1024, 1024);
                    break;
                case 2:
                    size = new Vector2(2048, 2048);
                    break;                
                case 3:
                    size = new Vector2(4096, 4096);
                    break;
                default:
                    return;
                    
            }
            ShadowFbo = new FBO((int)size.X, (int)size.Y, FramebufferAttachment.DepthAttachment,
                PixelInternalFormat.DepthComponent16, new TextureParameter
                {
                    Name = TextureParameterName.TextureCompareMode,
                    Value = (int) TextureCompareMode.CompareRefToTexture
                });       
        }

        public static void Dispose()
        {
            ShadowFbo?.Dispose();
        }
    }
}
