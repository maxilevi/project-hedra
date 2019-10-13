/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 26/01/2016
 * Time: 01:55 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using Hedra.Core;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Rendering.UI;
using Hedra.Game;
using OpenToolkit.Mathematics;
using Hedra.Engine.Core;
using Hedra.Engine.Windowing;

namespace Hedra.Rendering
{
    public static class Graphics2D
    {
        public static ITexture2DProvider Provider { get; set; } = new Texture2DProvider();

        public static uint LoadTexture(BitmapObject BitmapObject, bool UseCache)
        {
            return LoadTexture(BitmapObject, TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.ClampToBorder, UseCache);
        }

        public static uint LoadTexture(BitmapObject BitmapObject, TextureMinFilter Min = TextureMinFilter.Linear, TextureMagFilter Mag = TextureMagFilter.Linear, TextureWrapMode Wrap = TextureWrapMode.ClampToBorder, bool UseCache = true)
        {
            if (UseCache && TextureRegistry.Contains(BitmapObject.Path, Min, Mag, Wrap, out var cachedId))
                return cachedId;
            
            var id = Provider.LoadTexture(BitmapObject, Min, Mag, Wrap);
            if(UseCache)
                TextureRegistry.Add(id, BitmapObject.Path, Min, Mag, Wrap);
            else
                TextureRegistry.Unregister(id);

            if(Engine.Loader.Hedra.MainThreadId != Thread.CurrentThread.ManagedThreadId && !GameSettings.TestingMode)
                Log.WriteLine($"[Error] Texture being created outside of the GL thread");
            return id;
        }

        public static Vector2 ToRelativeSize(this Vector2 Size)
        {
            return new Vector2(Size.X / (float)GameSettings.Width, Size.Y / (float)GameSettings.Height);
        }
        
        public static Vector2 ToPixelSize(this Vector2 Size)
        {
            return new Vector2(Size.X * GameSettings.Width, Size.Y * GameSettings.Height);
        }

        #region NonGL

        public static Vector2 TextureSize(Bitmap bmp)
        {
            return new Vector2(bmp.Width, bmp.Height).ToRelativeSize();
        }

        public static Vector2 As1920x1080(this Vector2 Size)
        {
            return new Vector2(Size.X / 1920 * GameSettings.Width, Size.Y / 1080 * GameSettings.Height);
        }
        
        public static float As1920x1080(this int Size)
        {
            return (Size / 1920f) * GameSettings.Width;
        }
        
        public static Vector2 SizeFromAssets(string Path)
        {
            return TextureSize( new Bitmap( new MemoryStream(AssetManager.ReadBinary(Path, AssetManager.AssetsResource))));
        }

        public static uint LoadFromAssets(string Path, TextureMinFilter Min = TextureMinFilter.Linear, TextureMagFilter Mag = TextureMagFilter.Linear, TextureWrapMode Wrap = TextureWrapMode.ClampToBorder)
        {
            Log.WriteLine($"Resolving Path: {Path}", LogType.System);
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
        
        public static Vector2 MeasureString(string Text, Font TextFont)
        {
            if(Text == string.Empty) return Vector2.Zero;
            var size = TextProvider.CalculateNeededSize(new TextParams(new[] {Text}, new[] {0}, new[] {TextFont}, null, null));
            return new Vector2(size.Width / GameSettings.Width, size.Height / GameSettings.Height);
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
            }, TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Repeat);
        }

        public static Bitmap ReplaceColor(Bitmap Bmp, Color Original, Color Replacement)
        {
            return ReplaceColor(Bmp, C => C.R == Original.R && C.G == Original.G && C.B == Original.B, Replacement);
        }
        
        public static Bitmap ReplaceColor(Bitmap Bmp, Predicate<Color> Match, Color Replacement)
        {
            var data = Bmp.LockBits(new Rectangle(0,0,Bmp.Width,Bmp.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            unsafe
            {
                var dataPtr = (byte*)data.Scan0;
                var stride = data.Stride;
                if(dataPtr == null) throw new ArgumentNullException("dataPrt cannot be null");
                for (var y = 0; y < Bmp.Height; y++)
                {
                    for (var x = 0; x < Bmp.Width; x++)
                    {
                        if(!Match(Color.FromArgb(dataPtr[x * 4 + y * stride + 3], dataPtr[x * 4 + y * stride + 2], dataPtr[x * 4 + y * stride + 1], dataPtr[x * 4 + y * stride + 0])))
                            continue;
                        dataPtr[x * 4 + y * stride] = Replacement.B;
                        dataPtr[x * 4 + y * stride + 1] = Replacement.G;
                        dataPtr[x * 4 + y * stride + 2] = Replacement.R;
                    }
                }
            }
            Bmp.UnlockBits(data);
            return Bmp;
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
            Renderer.Provider.DeleteTextures(TextureRegistry.Count, TextureRegistry.All);
        }
    }
    
    public enum GradientType{
        Diagonal,
        LeftRight,
        TopBot,
        Center
    }
}
