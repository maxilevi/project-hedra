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
using Hedra.Engine.Rendering.Core;

namespace Hedra.Engine.Rendering.Effects
{
    /// <summary>
    /// Description of BloomFilter.
    /// </summary>
    public class BlurFilter : Filter
    {
        
        private static readonly Shader HBlurShader;
        private static readonly Shader VBlurShader;

        static BlurFilter()
        {
            HBlurShader = Shader.Build("Shaders/HBlur.vert", "Shaders/Blur.frag");
            VBlurShader = Shader.Build("Shaders/VBlur.vert", "Shaders/Blur.frag");
        }

        public override void Pass(FBO Src, FBO Dst){
            
            Dst.Bind();
            HBlurShader.Bind();
            this.DrawQuad(HBlurShader, Src.TextureId[0], 0);
            HBlurShader.Unbind();
            Dst.Unbind();

            Src.Bind();
            
            VBlurShader.Bind();
            this.DrawQuad(VBlurShader, Dst.TextureId[0], 0);
            VBlurShader.Unbind();
            
            Src.Unbind();
            
            Dst.Bind();
            MainFBO.DrawQuad(Src.TextureId[0]);
            Dst.Unbind();
        }
        
        public override void DrawQuad(Shader DrawingShader, uint TexID, uint Additive = 0, bool Flipped = false)
        {
            Renderer.Disable(EnableCap.DepthTest);

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
