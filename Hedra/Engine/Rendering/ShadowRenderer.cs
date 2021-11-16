/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 17/12/2016
 * Time: 07:06 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Numerics;
using Hedra.Core;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.IO;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Rendering.Frustum;
using Hedra.Engine.Windowing;
using Hedra.Game;
using Hedra.Numerics;

namespace Hedra.Engine.Rendering
{
    /// <summary>
    ///     Description of ShadowRenderer.
    /// </summary>
    public static class ShadowRenderer
    {
        private static float _shadowDistance;
        private static uint _prevFbo;

        static ShadowRenderer()
        {
            LightPosition = new Vector3(-500, 700, 0);
            Shader = Shader.Build("Shaders/Shadows.vert", "Shaders/Shadows.frag");
            ShadowDistance = 1400;
            SetQuality(GameSettings.ShadowQuality);
        }

        public static Vector3 LightPosition { get; set; }
        public static Shader Shader { get; set; }
        public static FBO ShadowFbo { get; set; }
        public static Matrix4x4 DepthProj { get; set; }
        public static Matrix4x4 DepthView { get; set; }
        public static Matrix4x4 ShadowMvp { get; set; }

        public static float ShadowDistance
        {
            get => _shadowDistance;
            set
            {
                if (_shadowDistance == value) return;
                _shadowDistance = value;
                DepthProj = Matrix4x4.CreateOrthographicOffCenter(-_shadowDistance, _shadowDistance,
                    -_shadowDistance, _shadowDistance,
                    -_shadowDistance, _shadowDistance);
            }
        }

        public static void Bind()
        {
            _prevFbo = Renderer.FBOBound;
            ShadowFbo.Bind();

            var position = GameManager.Player.View.CameraEyePosition.Xz().ToVector3() +
                           Vector3.UnitY * (GameManager.Player.Position.Y + 512);
            var normalizedLight = new Vector3(LightPosition.X, LightPosition.Y, LightPosition.Z).NormalizedFast();
            var invertedLight = Matrix4x4.CreateRotationY(SkyManager.DayTime / 24000f * 360f * Mathf.Radian).Inverted()
                .Transposed();
            normalizedLight =
                Vector3.TransformNormal(new Vector3(normalizedLight.X, normalizedLight.Y, normalizedLight.Z),
                    invertedLight);

            DepthView = Matrix4x4.CreateLookAt(normalizedLight + position, position, Vector3.UnitY);
            ShadowMvp = DepthView * DepthProj;

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
            Vector2 size;
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
                    Value = (int)TextureCompareMode.CompareRefToTexture
                });
        }

        public static void Dispose()
        {
            ShadowFbo?.Dispose();
        }
    }
}