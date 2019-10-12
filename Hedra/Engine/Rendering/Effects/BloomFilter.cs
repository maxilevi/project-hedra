/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/08/2016
 * Time: 01:16 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Game;
using Hedra.Engine.Rendering.Core;
using Hedra.Game;

namespace Hedra.Engine.Rendering.Effects
{
    /// <summary>
    /// Description of BloomFilter.
    /// </summary>
    public class BloomFilter : Filter
    {
        private static readonly Shader Bloom;
        private static readonly Shader HBlurShader;
        private static readonly Shader VBlurShader;
        public FBO HBloomFbo = new FBO(GameSettings.Width / 4, GameSettings.Height / 4);
        public FBO VBloomFbo = new FBO(GameSettings.Width / 4, GameSettings.Height / 4);
        public int TopColorUniform, BotColorUniform, HeightUniform;

        static BloomFilter()
        {
            Bloom = Shader.Build("Shaders/Bloom.vert", "Shaders/Bloom.frag");
            HBlurShader = Shader.Build("Shaders/HBlur.vert", "Shaders/Blur.frag");
            VBlurShader = Shader.Build("Shaders/VBlur.vert", "Shaders/Blur.frag");
        }

        
        public override void Pass(FBO Src, FBO Dst){
            HBloomFbo.Bind();
            Bloom.Bind();
            Bloom["Modifier"] = GameSettings.BloomModifier;
            Renderer.Clear(ClearBufferMask.ColorBufferBit);
            this.DrawQuad(Bloom, Src.TextureId[0]);
            
            Bloom.Unbind();
            HBloomFbo.Unbind();
            
            VBloomFbo.Bind();
            HBlurShader.Bind();

            Renderer.Clear(ClearBufferMask.ColorBufferBit);
            this.DrawQuad(HBlurShader, HBloomFbo.TextureId[0]);
            
            HBlurShader.Unbind();
            VBloomFbo.Unbind();
            
            HBloomFbo.Bind();
            VBlurShader.Bind();

            Renderer.Clear(ClearBufferMask.ColorBufferBit);
            this.DrawQuad(VBlurShader, VBloomFbo.TextureId[0]);
            
            VBlurShader.Unbind();
            HBloomFbo.Unbind();
            
            Dst.Bind();
            MainFBO.DefaultShader.Bind();
            Renderer.Clear(ClearBufferMask.ColorBufferBit);
            this.DrawQuad(MainFBO.DefaultShader, HBloomFbo.TextureId[0], 0);
            MainFBO.DefaultShader.Unbind();
            Dst.Unbind();
        }

        public override void Dispose()
        {
            HBloomFbo.Dispose();
            VBloomFbo.Dispose();
        }
    }
}
