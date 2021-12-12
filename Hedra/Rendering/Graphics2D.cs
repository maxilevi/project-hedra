/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 26/01/2016
 * Time: 01:55 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Windowing;
using Hedra.Game;
using Hedra.Numerics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Hedra.Rendering
{
    public static class Graphics2D
    {
        private static ConcurrentDictionary<string, Vector2> _sizeCache = new ConcurrentDictionary<string, Vector2>();
        public static ITexture2DProvider Provider { get; set; } = new Texture2DProvider();

        public static uint LoadTexture(BitmapObject BitmapObject, bool UseCache)
        {
            return LoadTexture(BitmapObject, TextureMinFilter.Linear, TextureMagFilter.Linear,
                TextureWrapMode.ClampToBorder, UseCache);
        }

         
        public static Vector2 MeasureString(string Text, Font TextFont)
        {
            if (Text == string.Empty) return Vector2.Zero;
            var size = TextProvider.CalculateNeededSize(new TextParams(new[] { Text }, new[] { 0 }, new[] { TextFont },
                    null, null));
            return new Vector2(size.Width / GameSettings.Width, size.Height / GameSettings.Height);
        }
        
        public static uint LoadTexture(BitmapObject BitmapObject, TextureMinFilter Min = TextureMinFilter.Linear,
            TextureMagFilter Mag = TextureMagFilter.Linear, TextureWrapMode Wrap = TextureWrapMode.ClampToBorder,
            bool UseCache = true)
        {
            if (UseCache && TextureRegistry.Contains(BitmapObject.Path, Min, Mag, Wrap, out var cachedId))
                return cachedId;

            var id = Provider.LoadTexture(BitmapObject, Min, Mag, Wrap);
            if (UseCache)
                TextureRegistry.Add(id, BitmapObject.Path, Min, Mag, Wrap);
            else
                TextureRegistry.Unregister(id);

            if (Engine.Loader.Hedra.RenderingThreadId != Thread.CurrentThread.ManagedThreadId && !GameSettings.TestingMode)
                Log.WriteLine("[Error] Texture being created outside of the GL thread");
            return id;
        }

        public static Vector2 ToRelativeSize(this Vector2 Size)
        {
            return new Vector2(Size.X / GameSettings.Width, Size.Y / GameSettings.Height);
        }

        public static void Dispose()
        {
            Renderer.Provider.DeleteTextures(TextureRegistry.Count, TextureRegistry.All);
        }

        #region NonGL

        public static Vector2 TextureSize(Image bmp)
        {
            return new Vector2(bmp.Width, bmp.Height).ToRelativeSize();
        }

        public static Vector2 As1920x1080(this Vector2 Size)
        {
            var targetAspect = 1f / 1080 * GameSettings.Height;
            return new Vector2(Size.X * targetAspect, Size.Y * targetAspect);
        }

        public static float As1920x1080(this int Size)
        {
            return Size / 1920f * GameSettings.Width;
        }

        public static Vector2 SizeFromAssets(string Path)
        {
            if (_sizeCache.TryGetValue(Path, out var size))
                return size;

            size = TextureSize(Image.Load(new MemoryStream(AssetManager.ReadBinary(Path, AssetManager.AssetsResource))));
            _sizeCache.TryAdd(Path, size);
            return size;
        }

        public static uint LoadFromAssets(string Path, TextureMinFilter Min = TextureMinFilter.Linear,
            TextureMagFilter Mag = TextureMagFilter.Linear, TextureWrapMode Wrap = TextureWrapMode.ClampToBorder)
        {
            Log.WriteLine($"Resolving Path: {Path}", LogType.System);
            var id = LoadTexture(new BitmapObject
            {
                Bitmap = Image.Load<Rgba32>(new MemoryStream(AssetManager.ReadBinary(Path, AssetManager.AssetsResource))),
                Path = Path
            }, Min, Mag, Wrap);
            Log.WriteLine($"Loading Texture: {Path} Id={id}", LogType.System);
            return id;
        }

        public static Image<Rgba32> LoadBitmapFromAssets(string Path)
        {
            return Image.Load<Rgba32>(new MemoryStream(AssetManager.ReadBinary(Path, AssetManager.AssetsResource)));
        }

        public static uint ColorTexture(Vector4 TextureColor)
        {
            var textureColor = TextureColor.ToColor();
            var bmp = new Image<Rgba32>(2, 2);
            bmp[0, 0] = textureColor;
            bmp[0, 1] = textureColor;
            bmp[1, 0] = textureColor;
            bmp[1, 1] = textureColor;
            return LoadTexture(new BitmapObject
            {
                Bitmap = bmp,
                Path = $"UI:Color:{TextureColor}"
            }, TextureMinFilter.Linear, TextureMagFilter.Linear, TextureWrapMode.Repeat);
        }

        public static Image<Rgba32> ReplaceColor(Image<Rgba32> Bmp, Color Original, Color Replacement)
        {
            var original = Original.ToPixel<Rgba32>();
            return ReplaceColor(Bmp, C =>
            {
                var p = C.ToPixel<Rgba32>();
                return p.R == original.R && p.G == original.G && p.B == original.B;
            }, Replacement);
        }

        public static Image<Rgba32> ReplaceColor(Image<Rgba32> Bmp, Predicate<Color> Match, Color Replacement)
        {
            if (!Bmp.TryGetSinglePixelSpan(out var pixelSpan))
                throw new ArgumentException("Image is not contigous");

            var pixel = Replacement.ToPixel<Rgba32>();
            for (var i = 0; i < Bmp.Height * Bmp.Width; i++)
            {
                if (!Match(new Color(pixelSpan[i])))
                    continue;
                pixelSpan[i] = new Rgba32(pixel.R, pixel.G, pixel.B, pixelSpan[i].A);
            }

            return Image.LoadPixelData<Rgba32>(pixelSpan, Bmp.Width, Bmp.Height);
        }

        public static Image<Rgba32> CreateGradient(Color Color1, Color Color2, GradientType Type, Image<Rgba32> Bmp)
        {
            if (!Bmp.TryGetSinglePixelSpan(out var pixelSpan))
                throw new ArgumentException("Image is not contigous");
            
            for (var y = 0; y < Bmp.Height; y++)
            {
                for (var x = 0; x < Bmp.Width; x++)
                {
                    float LerpValue = 0;
                    if (Type == GradientType.LeftRight)
                        LerpValue = x / (float)Bmp.Width;

                    if (Type == GradientType.TopBot)
                        LerpValue = y / (float)Bmp.Height;

                    if (Type == GradientType.Diagonal)
                        LerpValue = Mathf.Clamp(
                            (new Vector2(0, 0) - new Vector2(x, y)).LengthFast() / (Bmp.Width + Bmp.Height), 0, 1);

                    if (Type == GradientType.Center)
                        LerpValue = Mathf.Clamp(
                            (new Vector2(Bmp.Width / 2, Bmp.Height / 2) - new Vector2(x, y)).LengthFast() /
                            (Bmp.Width + Bmp.Height), 0, 1);

                    var newColor = Color1.Lerp(Color2, LerpValue).ToPixel<Rgba32>();
                    pixelSpan[x + y * Bmp.Width] =
                        new Rgba32(newColor.R, newColor.G, newColor.B, (byte)Math.Max(0, (int)newColor.A));
                }
            }

            return Image.LoadPixelData<Rgba32>(pixelSpan, Bmp.Width, Bmp.Height);
        }

        #endregion
    }

    public enum GradientType
    {
        Diagonal,
        LeftRight,
        TopBot,
        Center
    }
}