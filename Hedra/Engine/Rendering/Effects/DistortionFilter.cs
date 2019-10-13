/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/11/2016
 * Time: 09:08 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Rendering.Core;
using OpenToolkit.Mathematics;
using Hedra.Engine.Core;
using Hedra.Engine.Rendering.UI;
using Hedra.Rendering;

namespace Hedra.Engine.Rendering.Effects
{
    /// <summary>
    /// Description of UnderWaterFilter.
    /// </summary>
    public class DistortionFilter : Filter
    {
        private static readonly Shader WaterEffect;
        private static readonly uint DuDvMapId;
        private int _duDvMapUniform;
        private int _timeUniform;

        static DistortionFilter()
        {
            WaterEffect = Shader.Build("Shaders/UnderWater.vert", "Shaders/Distortion.frag");
            DuDvMapId = Graphics2D.LoadFromAssets("Assets/FX/waterDuDvMap.png");
        }

        public DistortionFilter() : base(){
            _duDvMapUniform = Renderer.GetUniformLocation(WaterEffect.ShaderId, "DuDvMap");
            _timeUniform = Renderer.GetUniformLocation(WaterEffect.ShaderId, "Time");
        }

        public override void Pass(FBO Src, FBO Dst){
            Dst.Bind();
            
            WaterEffect.Bind();
            //Renderer.Uniform1(TimeUniform, WaterMeshBuffer.WaveMovement);
            
            Renderer.ActiveTexture(TextureUnit.Texture1);
            Renderer.BindTexture(TextureTarget.Texture2D, DuDvMapId);
            //Renderer.Uniform1(DuDvMapUniform, 1);
            
            DrawQuad(WaterEffect, Src.TextureId[0]);
            WaterEffect.Unbind();
            
            Dst.Unbind();
            Renderer.ActiveTexture(TextureUnit.Texture1);
            Renderer.BindTexture(TextureTarget.Texture2D, 0);
        }

        public override void Dispose()
        {

        }
    }
}
