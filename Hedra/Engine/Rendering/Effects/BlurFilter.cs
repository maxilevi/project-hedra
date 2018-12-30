/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/08/2016
 * Time: 01:16 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using OpenTK.Graphics.OpenGL4;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering.Effects
{
    /// <summary>
    /// Description of BloomFilter.
    /// </summary>
    public class BlurFilter : Filter
    {

        public override void Pass(FBO Src, FBO Dst)
        {
            
            BloomFilter.HBloomFbo.Bind();
            BloomFilter.HBlurShader.Bind();
            Renderer.Clear(ClearBufferMask.ColorBufferBit);
            this.DrawQuad(BloomFilter.HBlurShader, Src.TextureID[0], 0);
            BloomFilter.HBlurShader.Unbind();
            BloomFilter.HBloomFbo.UnBind();

            BloomFilter.VBloomFbo.Bind();         
            BloomFilter.VBlurShader.Bind();
            Renderer.Clear(ClearBufferMask.ColorBufferBit);
            this.DrawQuad(BloomFilter.VBlurShader, BloomFilter.HBloomFbo.TextureID[0], 0);
            BloomFilter.VBlurShader.Unbind();
            BloomFilter.VBloomFbo.UnBind();
            
            Dst.Bind();
            MainFBO.Shader.Bind();
            MainFBO.DrawQuad(BloomFilter.VBloomFbo.TextureID[0]);
            MainFBO.Shader.Unbind();
            Dst.UnBind();
        }
        
        public override void DrawQuad(Shader DrawingShader, uint TexID, uint Additive = 0, bool Flipped = false)
        {
            Renderer.Disable(EnableCap.DepthTest);

            DrawManager.UIRenderer.SetupQuad();

            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, TexID);

            DrawManager.UIRenderer.DrawQuad();

            Renderer.Enable(EnableCap.DepthTest);
            Renderer.Enable(EnableCap.CullFace);
        }

        public override void Dispose()
        {

        }
    }
}
