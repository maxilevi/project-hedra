/*
 * Author: Zaphyk
 * Date: 03/04/2016
 * Time: 01:02 p.m.
 *
 */
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using Hedra.Engine.Core;
using System.Collections.Generic;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.IO;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Windowing;
using Hedra.Game;
using Hedra.Numerics;
using PixelFormat = Hedra.Engine.Windowing.PixelFormat;

namespace Hedra.Engine.Rendering.Effects
{
    /// <summary>
    /// ONLY WORKS WITH SSAO
    /// </summary>
    public class DeferedRenderer
    {
        public const int SampleCount = 64;
        public FBO FirstPass;
        public FBO SecondPass;
        public FBO ThirdPass; 
        public FBO WaterPass;
        public Shader FirstPassShader;
        public Shader SecondPassShader;
        public Shader ThirdPassShader;
        public int SamplesUniform;
        public uint RandomTex;
        public float[] Samples;
        public int ColorSampler;
        public int PositionSampler;
        public int NormalSampler;
        public int RandomSampler;
        public int AOSampler;
        public int ProjectionUniform;
        public int Intensity;

        public DeferedRenderer()
        {
            FirstPassShader = Shader.Build("Shaders/SSAO.vert", "Shaders/SSAO-Pass1.frag");
            SecondPassShader = Shader.Build("Shaders/SSAO.vert", "Shaders/SSAO-Pass2.frag");
            ThirdPassShader = Shader.Build("Shaders/SSAO.vert", "Shaders/SSAO-Pass3.frag");

            var attachments = new FramebufferAttachment[3];
            attachments[0] = FramebufferAttachment.ColorAttachment0;
            attachments[1] = FramebufferAttachment.ColorAttachment1;
            attachments[2] = FramebufferAttachment.ColorAttachment2;

            var formats = new PixelInternalFormat[3];
            formats[0] = PixelInternalFormat.Rgba8;
            formats[1] = PixelInternalFormat.Rgba32f;
            formats[2] = PixelInternalFormat.Rgba16f;

            FirstPass = new FBO(new Size(GameSettings.Width, GameSettings.Height), attachments, formats, false, false, 0, true);
            ThirdPass = new FBO(GameSettings.Width / 2, GameSettings.Height / 2);
            SecondPass = new FBO(GameSettings.Width / 2, GameSettings.Height / 2);
            WaterPass = new FBO(GameSettings.Width / 4, GameSettings.Height / 4);

            #region SETUP UNIFORMS & TEXTURES
            SamplesUniform = Renderer.GetUniformLocation(FirstPassShader.ShaderId, "samples");
            PositionSampler = Renderer.GetUniformLocation(FirstPassShader.ShaderId, "Position1");
            NormalSampler = Renderer.GetUniformLocation(FirstPassShader.ShaderId, "Normal2");
            RandomSampler = Renderer.GetUniformLocation(FirstPassShader.ShaderId, "Random3");
            ProjectionUniform = Renderer.GetUniformLocation(FirstPassShader.ShaderId, "Projection");
            ColorSampler = Renderer.GetUniformLocation(ThirdPassShader.ShaderId, "ColorInput");
            AOSampler = Renderer.GetUniformLocation(ThirdPassShader.ShaderId, "SSAOInput");
            Intensity = Renderer.GetUniformLocation(FirstPassShader.ShaderId, "Intensity");
            
            var gen = new Random();
            var bmp = new Bitmap(4,4);
            for(var x = 0; x < 4; x++)
            {
                for(var y = 0; y < 4; y++)
                {
                    var value = new Vector3(gen.NextFloat(), gen.NextFloat(), 0.0f) * 2 - new Vector3(1, 1, 0);
                    var col = Color.FromArgb(255, (byte)(value.X * 255),(byte) (value.Y * 255), (byte) (value.Z * 255));
                    bmp.SetPixel(x,y, col);
                }
            }
            RandomTex = Renderer.GenTexture();
            
            Renderer.BindTexture(TextureTarget.Texture2D, RandomTex);
    
            var bmpData = bmp.LockBits(new Rectangle(0,0,bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
    
            Renderer.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmpData.Width, bmpData.Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
    
            bmp.UnlockBits(bmpData);
            //Bmp.Dispose();

            Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
            Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
            Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
            Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
            
            var vecSamples = new Vector3[SampleCount];
            for(var i = 0; i < SampleCount; i++)
            {
                vecSamples[i] = new Vector3( Utils.Rng.NextFloat() * 2.0f - 1.0f, Utils.Rng.NextFloat() * 2.0f - 1.0f, Utils.Rng.NextFloat() ).Normalized();
                vecSamples[i] *= Utils.Rng.NextFloat();
                var scale = i / (float) SampleCount;
                scale = Mathf.Lerp(0.1f, 1.0f, scale * scale);
                vecSamples[i] *= scale;
            }
            var fsamples = new List<float>();
            for(var i = 0; i < vecSamples.Length; i++)
            {
                fsamples.Add(vecSamples[i].X);
                fsamples.Add(vecSamples[i].Y);
                fsamples.Add(vecSamples[i].Z);
            }
            Samples = fsamples.ToArray();
            #endregion
        }

        public void Dispose()
        {
            FirstPass.Dispose();
            ThirdPass.Dispose();
            SecondPass.Dispose();
        }
    }
}
