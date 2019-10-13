/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/08/2016
 * Time: 01:17 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Core;
using OpenToolkit.Mathematics;
using Hedra.Engine.Core;
using Hedra.Engine.Windowing;

namespace Hedra.Engine.Rendering.Effects
{
    /// <summary>
    /// Description of UnderWaterFilter.
    /// </summary>
    public class UnderWaterFilter : Filter
    {
        private static readonly Shader WaterEffect;
        public Vector4 Multiplier { get; set; }

        static UnderWaterFilter()
        {
            WaterEffect = Shader.Build("Shaders/UnderWater.vert", "Shaders/UnderWater.frag");
        }

        public UnderWaterFilter() {
            Multiplier = new Vector4(1f, 1f, 1f, 1);
        }

        public override void Pass(FBO Src, FBO Dst){
            Dst.Bind();
            
            WaterEffect.Bind();
            //WaterEffect["Time"] = WaterMeshBuffer.WaveMovement;
            WaterEffect["Multiplier"] = Multiplier;
            this.DrawQuad(WaterEffect, Src.TextureId[0]);
            WaterEffect.Unbind();
            
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
            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, 0);
        }

        public override void Dispose()
        {

        }
    }
}
