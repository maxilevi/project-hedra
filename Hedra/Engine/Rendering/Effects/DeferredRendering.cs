/*
 * Author: Zaphyk
 * Date: 03/04/2016
 * Time: 01:02 p.m.
 *
 */
using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace Hedra.Engine.Rendering.Effects
{
	/// <summary>
	/// ONLY WORKS WITH SSAO
	/// </summary>
	public class DeferedRenderer
	{
		public FBO FirstPass;
		public FBO SecondPass;
	    public FBO ThirdPass; 
		public Shader FirstPassShader;
		public Shader SecondPassShader;
	    public Shader ThirdPassShader;
		public int SamplesUniform;
		public int RandomTex;
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

            FramebufferAttachment[] Attachments = new FramebufferAttachment[3];
			Attachments[0] = FramebufferAttachment.ColorAttachment0;
			Attachments[1] = FramebufferAttachment.ColorAttachment1;
			Attachments[2] = FramebufferAttachment.ColorAttachment2;

            PixelInternalFormat[] Formats = new PixelInternalFormat[3];
		    Formats[0] = PixelInternalFormat.Rgba8;
		    Formats[1] = PixelInternalFormat.Rgba32f;
		    Formats[2] = PixelInternalFormat.Rgba16f;

            FirstPass = new FBO(new Size(GameSettings.Width, GameSettings.Height), Attachments, Formats, false, false, 0, true);
		    ThirdPass = new FBO(GameSettings.Width / 2, GameSettings.Height / 2);
		    SecondPass = new FBO(GameSettings.Width, GameSettings.Height);

            #region SETUP UNIFORMS & TEXTURES
            SamplesUniform = Renderer.GetUniformLocation(FirstPassShader.ShaderId, "samples");
			PositionSampler = Renderer.GetUniformLocation(FirstPassShader.ShaderId, "Position1");
			NormalSampler = Renderer.GetUniformLocation(FirstPassShader.ShaderId, "Normal2");
			RandomSampler = Renderer.GetUniformLocation(FirstPassShader.ShaderId, "Random3");
			ProjectionUniform = Renderer.GetUniformLocation(FirstPassShader.ShaderId, "Projection");
			ColorSampler = Renderer.GetUniformLocation(ThirdPassShader.ShaderId, "ColorInput");
			AOSampler = Renderer.GetUniformLocation(ThirdPassShader.ShaderId, "SSAOInput");
			Intensity = Renderer.GetUniformLocation(FirstPassShader.ShaderId, "Intensity");
			
			Random Gen = new Random();
			Bitmap Bmp = new Bitmap(4,4);
			for(int x = 0; x < 4; x++){
				for(int y = 0; y < 4; y++){
					Vector3 Value = new Vector3( Gen.NextFloat(), Gen.NextFloat(), 0.0f);
					Color Col = Color.FromArgb(255, (byte)(Value.X * 255),(byte) (Value.Y * 255), (byte) (Value.Z * 255) );
					Bmp.SetPixel(x,y, Col);
				}
			}
			RandomTex = Renderer.GenTexture();
			
			Renderer.BindTexture(TextureTarget.Texture2D, RandomTex);
	
	        BitmapData bmp_data = Bmp.LockBits(new Rectangle(0,0,Bmp.Width, Bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
	
	        Renderer.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
	            OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
	
	        Bmp.UnlockBits(bmp_data);
	        //Bmp.Dispose();

			Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
			Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
			Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
			Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
			
			Vector3[] VecSamples = new Vector3[16];
			for(int i = 0; i < 16; i++){
				VecSamples[i] = new Vector3( Utils.Rng.NextFloat() * 2.0f - 1.0f, Utils.Rng.NextFloat() * 2.0f - 1.0f, Utils.Rng.NextFloat() );
				VecSamples[i].Normalize();
				VecSamples[i] *= Utils.Rng.NextFloat();
				float Scale = i/16;
				Scale = Mathf.Lerp(0.1f, 1.0f, Scale * Scale);
				VecSamples[i] *= Scale;
			}
			List<float> fsamples = new List<float>();
			for(int i = 0; i < VecSamples.Length; i++){
				fsamples.Add(VecSamples[i].X);
				fsamples.Add(VecSamples[i].Y);
				fsamples.Add(VecSamples[i].Z);
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
