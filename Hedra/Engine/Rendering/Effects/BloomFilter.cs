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
using OpenTK;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Game;

namespace Hedra.Engine.Rendering.Effects
{
    /// <summary>
    /// Description of BloomFilter.
    /// </summary>
    public class BloomFilter : Filter
    {
        private static readonly Shader Bloom;
        public static readonly Shader HBlurShader;
        public static readonly Shader VBlurShader;
        public static readonly FBO HBloomFbo = new FBO(GameSettings.Width / 2, GameSettings.Height / 2);
        public static readonly FBO VBloomFbo = new FBO(GameSettings.Width / 2, GameSettings.Height / 2);

        static BloomFilter()
        {
            Bloom = Shader.Build("Shaders/Bloom.vert", "Shaders/Bloom.frag");
            HBlurShader = Shader.Build("Shaders/Blur.vert", "Shaders/HBlur.frag");
            VBlurShader = Shader.Build("Shaders/Blur.vert", "Shaders/VBlur.frag");
        }
        
        public override void Pass(FBO Src, FBO Dst)
        {
            HBloomFbo.Bind();
            Bloom.Bind();
            Bloom["Modifier"] = GameSettings.BloomModifier;
            Renderer.Clear(ClearBufferMask.ColorBufferBit);
            this.DrawQuad(Bloom, Src.TextureID[0]);
            
            Bloom.Unbind();
            HBloomFbo.UnBind();
            
            VBloomFbo.Bind();
            HBlurShader.Bind();
            HBlurShader["TexelSize"] = new Vector2(1.0f / VBloomFbo.Size.Width, 1.0f / VBloomFbo.Size.Height); 
            Renderer.Clear(ClearBufferMask.ColorBufferBit);
            this.DrawQuad(HBlurShader, HBloomFbo.TextureID[0]);
            
            HBlurShader.Unbind();
            VBloomFbo.UnBind();
            
            HBloomFbo.Bind();
            VBlurShader.Bind();
            VBlurShader["TexelSize"] = new Vector2(1.0f / VBloomFbo.Size.Width, 1.0f / VBloomFbo.Size.Height); 
            Renderer.Clear(ClearBufferMask.ColorBufferBit);
            this.DrawQuad(VBlurShader, VBloomFbo.TextureID[0]);
            
            VBlurShader.Unbind();
            HBloomFbo.UnBind();
            
            Dst.Bind();
            MainFBO.Shader.Bind();
            Renderer.Clear(ClearBufferMask.ColorBufferBit);
            this.DrawQuad(MainFBO.Shader, HBloomFbo.TextureID[0], 0);
            MainFBO.Shader.Unbind();
            Dst.UnBind();
        }

        public override void Dispose()
        {
            HBloomFbo.Dispose();
            VBloomFbo.Dispose();
        }
    }
}
