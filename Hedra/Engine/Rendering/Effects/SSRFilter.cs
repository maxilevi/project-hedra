using System;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Windowing;

namespace Hedra.Engine.Rendering.Effects
{
    public class SSRFilter : Filter
    {
        private static readonly Shader SSRShader = Shader.Build("Shaders/SSR.vert", "Shaders/SSR.frag");

        public override void Pass(FBO Src, FBO Dst)
        {
            Dst.Bind();
            Bind();

            DrawManager.UIRenderer.DrawQuad();

            UnBind();
            Dst.Unbind();
        }

        public void Bind()
        {
            SSRShader.Bind();

            SSRShader["projection"] = Renderer.ProjectionMatrix;
            SSRShader["view"] = Renderer.ModelViewMatrix;

            SSRShader["gPosition"] = 0;
            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, DrawManager.MainBuffer.Ssao.FirstPass.TextureId[1]);


            SSRShader["gNormal"] = 1;
            Renderer.ActiveTexture(TextureUnit.Texture1);
            Renderer.BindTexture(TextureTarget.Texture2D, DrawManager.MainBuffer.Ssao.FirstPass.TextureId[2]);

            SSRShader["gFinalImage"] = 2;
            Renderer.ActiveTexture(TextureUnit.Texture2);
            Renderer.BindTexture(TextureTarget.Texture2D, DrawManager.MainBuffer.FinalFbo.TextureId[0]);

            SSRShader["gColor"] = 3;
            Renderer.ActiveTexture(TextureUnit.Texture3);
            Renderer.BindTexture(TextureTarget.Texture2D, DrawManager.MainBuffer.Ssao.FirstPass.TextureId[0]);

            Renderer.Disable(EnableCap.DepthTest);
        }

        public void UnBind()
        {
            SSRShader.Unbind();
            Renderer.Enable(EnableCap.DepthTest);
            Renderer.Enable(EnableCap.CullFace);

            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, 0);
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}