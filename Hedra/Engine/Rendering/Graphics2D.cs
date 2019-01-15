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
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.IO;

namespace Hedra.Engine.Rendering
{
    public static class Graphics2D
    {
        public static ITexture2DProvider Provider { get; set; } = new Texture2DProvider();
        public static readonly List<uint> Textures = new List<uint>();

        public static uint LoadTexture(BitmapObject BitmapObject, TextureMinFilter Min = TextureMinFilter.Linear, TextureMagFilter Mag = TextureMagFilter.Linear, TextureWrapMode Wrap = TextureWrapMode.ClampToBorder)
        {
            var id = Provider.LoadTexture(BitmapObject, Min, Mag, Wrap);
            Textures.Add(id);
            if(Loader.Hedra.MainThreadId != Thread.CurrentThread.ManagedThreadId)
                Log.WriteLine($"[Error] Texture being created outside of the GL thread");
            return id;
        }

        public static Vector2 ToRelativeSize(this Vector2 Size)
        {
            return new Vector2(Size.X / (float)GameSettings.Width, Size.Y / (float)GameSettings.Height);
        }

        #region NonGL

        public static Vector2 TextureSize(Bitmap bmp)
        {
            return new Vector2(bmp.Width, bmp.Height).ToRelativeSize();
        }

        public static Bitmap Clone(Bitmap Original)
        {
            return Original.Clone(new RectangleF(0, 0, Original.Width, Original.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        }
        
        public static Vector2 SizeFromAssets(string Path)
        {
            return TextureSize( new Bitmap( new MemoryStream(AssetManager.ReadBinary(Path, AssetManager.AssetsResource))));
        }

        public static uint LoadFromAssets(string Path, TextureMinFilter Min = TextureMinFilter.Linear, TextureMagFilter Mag = TextureMagFilter.Linear, TextureWrapMode Wrap = TextureWrapMode.ClampToBorder)
        {
            var id = LoadTexture(new BitmapObject
            {
                Bitmap = new Bitmap(new MemoryStream(AssetManager.ReadBinary(Path, AssetManager.AssetsResource))),
                Path = Path
            }, Min, Mag, Wrap);
            Log.WriteLine($"Loading Texture: {Path} Id={id}", LogType.System);
            return id;
        }
        
        public static Bitmap LoadBitmapFromAssets(string Path)
        {
            return new Bitmap( new MemoryStream(AssetManager.ReadBinary(Path, AssetManager.AssetsResource)));
        }
        
        public static Vector2 LineSize(string Text, Font F)
        {
            var bmp = new Bitmap(1,1);
            using (var graphics = Graphics.FromImage(bmp))
            { 
                var s = graphics.MeasureString(Text, F);
                return new Vector2(s.Width, s.Height);
                
            }  
        }
        
        public static uint ColorTexture(Vector4 TextureColor)
        {
            var textureColor = TextureColor.ToColor();
            var bmp = new Bitmap(2,2);
            bmp.SetPixel(0, 0, textureColor);
            bmp.SetPixel(0, 1, textureColor);
            bmp.SetPixel(1, 0, textureColor);
            bmp.SetPixel(1, 1, textureColor);
            return LoadTexture(new BitmapObject
            {
                Bitmap = bmp,
                Path = $"UI:Color:{TextureColor}"
            }, TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear, TextureWrapMode.Repeat);
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
                            if(Type == GradientType.LeftRight)
                                LerpValue = (float) ( (float) x / (float) Bmp.Width);
                            
                            if(Type == GradientType.TopBot)
                                LerpValue = (float) ( (float) y / (float) Bmp.Height);
                            
                            if(Type == GradientType.Diagonal)
                                LerpValue = Mathf.Clamp( (new Vector2(0,0) - new Vector2(x,y)).LengthFast / (Bmp.Width+Bmp.Height), 0, 1);
                            
                            if(Type == GradientType.Center)
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

        public static void Dispose()
        {
            Renderer.DeleteTextures(Textures.Count, Textures.ToArray());
        }
    }
    
    public enum GradientType{
        Diagonal,
        LeftRight,
        TopBot,
        Center
    }
}
