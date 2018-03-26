/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 26/01/2016
 * Time: 01:55 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Management;
using System.IO; 
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace Hedra.Engine.Rendering
{
	public static class Graphics2D
	{
		public static List<uint> Textures = new List<uint>();
		public static Bitmap RoundedRectangle = new Bitmap(new MemoryStream(AssetManager.ReadBinary("Assets/Background.png",AssetManager.DataFile3)));

        public static void UpdateTexture(uint ID, Bitmap bmp)
		{
	        GL.BindTexture(TextureTarget.Texture2D, ID);
	        BitmapData bmp_data = bmp.LockBits(new Rectangle(0,0,bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
	
	        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
	            OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
	        
	        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
	        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

	        bmp.UnlockBits(bmp_data);
		}
		public static uint LoadTexture(Bitmap bmp, TextureMinFilter Min, TextureMagFilter Mag, TextureWrapMode Wrap)
		{
            uint id;
			GL.GenTextures(1, out id);
	        GL.BindTexture(TextureTarget.Texture2D, id);
	
	        BitmapData bmp_data = bmp.LockBits(new Rectangle(0,0,bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
	
	        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
	            OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
	
	        bmp.UnlockBits(bmp_data);
	
	        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)Min);
	        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)Mag);
		    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)Wrap);
		    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)Wrap);

            bmp.Dispose();
	        Textures.Add(id);
	        
	        return id;
	    }
		
		public static uint LoadTexture(Bitmap bmp){
			return Graphics2D.LoadTexture(bmp, TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.ClampToBorder);
		}
		
		public static uint CreateFromText(string Text, Color FontColor, Font F){
			Bitmap TextBitmap = new Bitmap(1,1);
			SolidBrush Brush = new SolidBrush(FontColor);
			using (Graphics Graphics = Graphics.FromImage(TextBitmap))
			{
					SizeF Size = Graphics.MeasureString(Text, F);
					TextBitmap = new Bitmap(TextBitmap, (int) Math.Ceiling(Size.Width), (int) Math.Ceiling(Size.Height));
			}
			using (Graphics Graphics = Graphics.FromImage(TextBitmap))
			{
				Graphics.Clear(System.Drawing.Color.Transparent);
				Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
				Graphics.DrawString(Text, F, Brush, 0, 0);
			}
			return Graphics2D.LoadTexture(TextBitmap);
		}

	    public static Vector2 ToRelativeSize(this Vector2 Size)
	    {
	        return new Vector2(Size.X / GameSettings.Width, Size.Y / GameSettings.Height);
	    }

        #region NonGL

        public static Vector2 TextureSize(Bitmap bmp)
	    {
	        return new Vector2((float)bmp.Width / (float)GameSettings.Width, (float)bmp.Height / (float)GameSettings.Height);
	    }

        public static Bitmap Clone(Bitmap Original)
	    {
	        return Original.Clone(new RectangleF(0, 0, Original.Width, Original.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
	    }

        public static Bitmap AddBitmap(Bitmap Bmp1, Bitmap Bmp2){
			BitmapData Data1 = Bmp1.LockBits(new Rectangle(0,0,Bmp1.Width,Bmp1.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			BitmapData Data2 = Bmp2.LockBits(new Rectangle(0,0,Bmp2.Width,Bmp2.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			  unsafe
		      {
		          byte* DataPtr1 = (byte*)Data1.Scan0;
		          byte* DataPtr2 = (byte*)Data2.Scan0;
		          int Stride1 = Data1.Stride;
		          int Stride2 = Data2.Stride;
		          
		          for (int y = 0; y < Bmp1.Height; y++)
		          {
		              for (int x = 0; x < Bmp1.Width; x++) 
		              {
		              	
		              	if(DataPtr2[(x * 4) + y * Stride2 + 3] > 0){
			              	  DataPtr1[(x * 4) + y * Stride1] =  DataPtr2[(x * 4) + y * Stride2]; // Red
			                  DataPtr1[(x * 4) + y * Stride1 + 1] = DataPtr2[(x * 4) + y * Stride2 + 1]; // Blue
			                  DataPtr1[(x * 4) + y * Stride1 + 2] = DataPtr2[(x * 4) + y * Stride2 + 2]; // Green
			                 // DataPtr[(x * 4) + y * Stride + 3] = DataPtr[(x * 4) + y * Stride+3]; // Red
		              	}
		
		              }
		          }
		      }
			Bmp1.UnlockBits(Data1);
			Bmp2.UnlockBits(Data2);
			return Bmp1;
		}
		
		public static Vector2 SizeFromAssets(string Path){
			return Graphics2D.TextureSize( new Bitmap( new MemoryStream(AssetManager.ReadBinary(Path, AssetManager.DataFile3))));
		}
		
		public static uint LoadFromAssets(string Path){
			return Graphics2D.LoadTexture( new Bitmap( new MemoryStream(AssetManager.ReadBinary(Path, AssetManager.DataFile3))));
		}
		
		public static Bitmap LoadBitmapFromAssets(string Path){
			return new Bitmap( new MemoryStream(AssetManager.ReadBinary(Path, AssetManager.DataFile3)));
		}
		
		public static Vector2 LineSize(string Text, Font F){
			Bitmap Bmp = new Bitmap(1,1);
			using (Graphics Graphics = Graphics.FromImage(Bmp))
			{ 
				SizeF S = Graphics.MeasureString(Text, F);
				return new Vector2(S.Width, S.Height);
				
			}  
		}
		
		public static uint ColorTexture(Vector4 TextureColor)
		{
		    var textureColor = TextureColor.ToColor();
			Bitmap Bmp = new Bitmap(2,2);
			Bmp.SetPixel(0, 0, textureColor);
		    Bmp.SetPixel(0, 1, textureColor);
		    Bmp.SetPixel(1, 0, textureColor);
		    Bmp.SetPixel(1, 1, textureColor);
            return LoadTexture(Bmp, TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear, TextureWrapMode.Repeat);
		}
		
		public static Bitmap ReColorMask(Color NewColor, Bitmap Mask){
			BitmapData Data = Mask.LockBits(new Rectangle(0,0,Mask.Width,Mask.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			  unsafe
		      {
		          byte* DataPtr = (byte*)Data.Scan0;
		          int Stride = Data.Stride;
		          
		          for (int y = 0; y < Mask.Height; y++)
		          {
		              for (int x = 0; x < Mask.Width; x++)
		              {
		              	  DataPtr[(x * 4) + y * Stride] = NewColor.B; // Red
		                  DataPtr[(x * 4) + y * Stride + 1] = NewColor.G; // Blue
		                  DataPtr[(x * 4) + y * Stride + 2] = NewColor.R; // Green
		                 // DataPtr[(x * 4) + y * Stride + 3] = DataPtr[(x * 4) + y * Stride+3]; // Red
		
		              }
		          }
		      }
			Mask.UnlockBits(Data);
			return Mask;
		}
		
		public static Bitmap CreateGradient(Color Color1, Color Color2, GradientType Type, Bitmap Bmp){
			BitmapData Data = Bmp.LockBits(new Rectangle(0,0,Bmp.Width,Bmp.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			  unsafe
		      {
		          byte* DataPtr = (byte*)Data.Scan0;
		          int Stride = Data.Stride;
		          
		          for (int y = 0; y < Bmp.Height; y++)
		          {
		              for (int x = 0; x < Bmp.Width; x++)
		              {
		              	  float LerpValue = 0;
		              	  if(Type == GradientType.LEFT_RIGHT)
		              	  	LerpValue = (float) ( (float) x / (float) Bmp.Width);
		              	  
		              	  if(Type == GradientType.TOP_BOT)
		              	  	LerpValue = (float) ( (float) y / (float) Bmp.Height);
		              	  
		              	  if(Type == GradientType.DIAGONAL)
		              	  	LerpValue = Mathf.Clamp( (new Vector2(0,0) - new Vector2(x,y)).LengthFast / (Bmp.Width+Bmp.Height), 0, 1);
		              	  
		              	  if(Type == GradientType.CENTER)
		              	  	LerpValue = Mathf.Clamp( (new Vector2(Bmp.Width / 2, Bmp.Height / 2) - new Vector2(x,y)).LengthFast / (Bmp.Width+Bmp.Height), 0, 1);
		              	  
		              	  Color NewColor = Mathf.Lerp(Color1, Color2, LerpValue);
		              	  DataPtr[(x * 4) + y * Stride] = NewColor.B; // Red
		                  DataPtr[(x * 4) + y * Stride + 1] = NewColor.G; // Blue
		                  DataPtr[(x * 4) + y * Stride + 2] = NewColor.R; // Green
		                  DataPtr[(x * 4) + y * Stride + 3] = (byte) Math.Max( 0x00, (int) NewColor.A);
		
		              }
		          }
		      }
			Bmp.UnlockBits(Data);
			return Bmp;
		}
#endregion

        public static void Dispose(){
			GL.DeleteTextures(Textures.Count, Textures.ToArray());
		}
	}
	
	public enum GradientType{
		DIAGONAL,
		LEFT_RIGHT,
		TOP_BOT,
		CENTER
	}
}
