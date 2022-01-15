/*
 * Author: Zaphyk
 * Date: 22/02/2016
 * Time: 12:43 a.m.
 *
 */

using System.Numerics;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Rendering.Frustum;
using Hedra.Engine.Windowing;
using Hedra.Game;
using Hedra.Rendering;

namespace Hedra.Engine.Rendering.Effects
{
    /// <summary>
    ///     Do Nothing.
    /// </summary>
    public class MainFBO
    {
        public FBO AdditiveFbo;
        public BloomFilter Bloom;
        public BlurFilter Blur;
        public FBO Default;
        public DistortionFilter Distortion;
        public FBO FinalFbo;
        public DeferedRenderer Ssao;
        public SSRFilter SSR;
        public FBO SSRFBO;
        public UnderWaterFilter UnderWater;
        public FBO WaterFbo;

        static MainFBO()
        {
            DefaultShader = Shader.Build("Shaders/GUI.vert", "Shaders/GUI.frag");
            FXAAShader = Shader.Build("Shaders/GUI.vert", "Shaders/FXAA.frag");
        }

        public MainFBO()
        {
            Ssao = new DeferedRenderer();
            Default = new FBO(GameSettings.Width, GameSettings.Height, false, 0, FramebufferAttachment.ColorAttachment0,
                PixelInternalFormat.Rgba32f);
            FinalFbo = new FBO(GameSettings.Width, GameSettings.Height);
            AdditiveFbo = new FBO(GameSettings.Width, GameSettings.Height);
            SSRFBO = new FBO(GameSettings.Width, GameSettings.Height);
            WaterFbo = new FBO(GameSettings.Width, GameSettings.Height);

            Bloom = new BloomFilter();
            UnderWater = new UnderWaterFilter();
            Distortion = new DistortionFilter();
            Blur = new BlurFilter();
            SSR = new SSRFilter();
        }

        public static Shader DefaultShader { get; }
        public static Shader FXAAShader { get; }
        public bool Enabled { get; set; }

        public static MainFBO DefaultBuffer => DrawManager.MainBuffer;

        public void Draw()
        {
            #region Normal

            //Just paste the contents without any effect
            if (!GameSettings.SSAO)
            {
                FinalFbo.Bind();
                DefaultShader.Bind();
                DrawQuad(Default.TextureId[0]);
                DefaultShader.Unbind();
                FinalFbo.Unbind();

                if (GameSettings.BlurFilter)
                {
                    Default.Bind();
                    DefaultShader.Bind();
                    DrawQuad(FinalFbo.TextureId[0]);
                    DefaultShader.Unbind();
                    Default.Unbind();

                    //Clear it
                    FinalFbo.Bind();
                    Renderer.ClearColor(Colors.Transparent);
                    FinalFbo.Unbind();
                }
            }

            #endregion

            #region SSAO

            if (GameSettings.SSAO)
            {
                var DrawFBO = GameSettings.UnderWaterEffect || GameSettings.BlurFilter || GameSettings.DarkEffect
                    ? Default
                    : FinalFbo;

                Ssao.SecondPass.Bind();

                Ssao.FirstPassShader.Bind();

                Renderer.Enable(EnableCap.Blend);

                Renderer.ActiveTexture(TextureUnit.Texture0);
                Renderer.BindTexture(TextureTarget.Texture2D, Ssao.FirstPass.TextureId[1]);

                Renderer.ActiveTexture(TextureUnit.Texture1);
                Renderer.BindTexture(TextureTarget.Texture2D, Ssao.FirstPass.TextureId[2]);

                Renderer.ActiveTexture(TextureUnit.Texture2);
                Renderer.BindTexture(TextureTarget.Texture2D, Ssao.RandomTex);

                Renderer.Uniform1(Ssao.PositionSampler, 0);
                Renderer.Uniform1(Ssao.NormalSampler, 1);
                Renderer.Uniform1(Ssao.RandomSampler, 2);

                Renderer.UniformMatrix4x4(Ssao.ProjectionUniform, false, ref Culling.ProjectionMatrix);

                DrawManager.UIRenderer.DrawQuad();

                Ssao.FirstPassShader.Unbind();

                DrawFBO.Bind();
                Ssao.SecondPassShader.Bind();

                //Firstpass output
                Renderer.ActiveTexture(TextureUnit.Texture0);
                Renderer.BindTexture(TextureTarget.Texture2D, Ssao.SecondPass.TextureId[0]);
                
                Renderer.ActiveTexture(TextureUnit.Texture1);
                Renderer.BindTexture(TextureTarget.Texture2D, Ssao.FirstPass.TextureId[0]);

                Renderer.Uniform1(Ssao.AOSampler, 0);
                Renderer.Uniform1(Ssao.ColorSampler, 1);
                
                DrawManager.UIRenderer.DrawQuad();
                
                DrawFBO.Unbind();

                /*Ssao.ThirdPass.Unbind();
                DrawFBO.Bind();
                Ssao.ThirdPassShader.Bind();

                //Firstpass output
                Renderer.ActiveTexture(TextureUnit.Texture0);
                Renderer.BindTexture(TextureTarget.Texture2D, Ssao.ThirdPass.TextureId[0]);
                //Color texture
                Renderer.ActiveTexture(TextureUnit.Texture1);
                Renderer.BindTexture(TextureTarget.Texture2D, Ssao.FirstPass.TextureId[0]);

                Renderer.Uniform1(Ssao.AOSampler, 0);
                Renderer.Uniform1(Ssao.ColorSampler, 1);

                DrawManager.UIRenderer.DrawQuad();

                Ssao.ThirdPassShader.Unbind();
                DrawFBO.Unbind(); //Unbind is the same*/

                Renderer.Enable(EnableCap.CullFace);
                Renderer.Disable(EnableCap.Blend);
            }

            #endregion

            #region Bloom

            if (GameSettings.Bloom) Bloom.Pass(FinalFbo, AdditiveFbo);

            #endregion

            #region UnderWater

            if (GameSettings.UnderWaterEffect)
            {
                var underChunk = World.GetChunkAt(LocalPlayer.Instance.View.CameraEyePosition);
                UnderWater.Multiplier =
                    underChunk?.Biome?.Colors.WaterColor * .7f + Vector4.One * .3f ?? Colors.DeepSkyBlue;
                UnderWater.Pass(Default, FinalFbo);
            }

            #endregion

            #region Distortion

            if (GameSettings.DistortEffect)
            {
                //    Distortion.Pass(Default, FinalFBO);
            }

            #endregion

            #region Dark

            if (GameSettings.DarkEffect)
            {
                UnderWater.Multiplier = new Vector4(.4f, .4f, .4f, 1);
                UnderWater.Pass(Default, FinalFbo);
            }

            #endregion

            #region Blur

            if (GameSettings.BlurFilter) Blur.Pass(Default, FinalFbo);

            #endregion

            #region UnderWater & SSAO Flip

            if ((GameSettings.UnderWaterEffect || GameSettings.DarkEffect || GameSettings.BlurFilter) &&
                GameSettings.SSAO)
            {
                Default.Bind();
                DefaultShader.Bind();
                DrawQuad(FinalFbo.TextureId[0], 0, true);
                DefaultShader.Unbind();
                Default.Unbind();

                FinalFbo.Bind();
                DefaultShader.Bind();
                DrawQuad(Default.TextureId[0]);
                DefaultShader.Unbind();
                FinalFbo.Unbind();
            }

            #endregion

            if (GameSettings.UseSSR)
            {
                Ssao.FirstPass.Bind(false);
                World.Draw(WorldRenderType.Water);
                Ssao.FirstPass.Unbind();

                SSR.Pass(WaterFbo, SSRFBO);
                Default.Bind();
                DefaultShader.Bind();
                DrawQuad(FinalFbo.TextureId[0], SSRFBO.TextureId[0]);
                DefaultShader.Unbind();
                Default.Unbind();

                FinalFbo.Bind();
                DefaultShader.Bind();
                DrawQuad(Default.TextureId[0]);
                DefaultShader.Unbind();
                FinalFbo.Unbind();
            }

            if (GameSettings.DepthEffect)
            {
            }

            Culling.SetViewport(GameSettings.DeviceWidth, GameSettings.DeviceHeight);
            if (GameSettings.FXAA)
                DrawFXAAQuad(FinalFbo.TextureId[0], GameSettings.Bloom ? AdditiveFbo.TextureId[0] : 0);
            else
                DrawQuad(FinalFbo.TextureId[0], GameSettings.Bloom ? AdditiveFbo.TextureId[0] : 0);
            //DrawQuad(Ssao.ThirdPass.TextureId[0], 0);

            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, 0);
            Renderer.ActiveTexture(TextureUnit.Texture1);
            Renderer.BindTexture(TextureTarget.Texture2D, 0);
            Renderer.ActiveTexture(TextureUnit.Texture2);
            Renderer.BindTexture(TextureTarget.Texture2D, 0);
        }

        private static void DrawFXAAQuad(uint Texture, uint AdditiveTexture)
        {
            Renderer.Disable(EnableCap.DepthTest);
            Renderer.Disable(EnableCap.Blend);

            FXAAShader.Bind();

            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, Texture);
            FXAAShader["Texture"] = 0;

            Renderer.ActiveTexture(TextureUnit.Texture1);
            Renderer.BindTexture(TextureTarget.Texture2D, AdditiveTexture);
            FXAAShader["Additive"] = 1;

            FXAAShader["Scale"] = Vector2.One;
            FXAAShader["Position"] = Vector2.Zero;
            FXAAShader["Resolution"] = new Vector2(1.0f / GameSettings.Width, 1.0f / GameSettings.Height);

            DrawManager.UIRenderer.DrawQuad();

            Renderer.Enable(EnableCap.DepthTest);
            FXAAShader.Unbind();
        }

        public static void DrawQuad(uint TexID, uint Additive = 0, bool Flipped = false)
        {
            Renderer.Disable(EnableCap.DepthTest);
            Renderer.Disable(EnableCap.Blend);

            DefaultShader.Bind();

            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, TexID);
            DefaultShader["Texture"] = 0;

            Renderer.ActiveTexture(TextureUnit.Texture1);
            Renderer.BindTexture(TextureTarget.Texture2D, Additive);
            DefaultShader["Background"] = 1;

            DefaultShader["Scale"] = Vector2.One;
            DefaultShader["Position"] = Vector2.Zero;
            DefaultShader["Flipped"] = Flipped ? 1 : 0;

            DrawManager.UIRenderer.DrawQuad();

            Renderer.Enable(EnableCap.DepthTest);
            DefaultShader.Unbind();
        }

        public void Clear()
        {
        }

        public void CaptureData()
        {
            if (!GameSettings.SSAO)
                Default.Bind();
            else
                Ssao.FirstPass.Bind();
        }

        public void UnCaptureData()
        {
            if (!GameSettings.SSAO)
                Default.Unbind();
            else
                Ssao.FirstPass.Unbind(); //Unbind ids the same
        }

        public void Dispose()
        {
            Default.Dispose();
            AdditiveFbo.Dispose();
            FinalFbo.Dispose();
            Ssao.Dispose();
            Default.Dispose();
            Bloom.Dispose();
            Blur.Dispose();
            UnderWater.Dispose();
            Distortion.Dispose();
        }
    }
}