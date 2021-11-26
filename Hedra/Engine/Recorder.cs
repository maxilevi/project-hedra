/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 21/06/2016
 * Time: 11:06 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.IO;
using System.Runtime.InteropServices;
using Hedra.Core;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Steamworks;
using Hedra.Engine.Windowing;
using Hedra.Game;
using Hedra.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using PixelFormat = Hedra.Engine.Windowing.PixelFormat;

namespace Hedra.Engine
{
    /// <summary>
    ///     Description of Recorder.
    /// </summary>
    public static class Recorder
    {
        public static string SaveScreenshot(string Path)
        {
            Steam.Instance.CallIf(S => S.Screenshots.Trigger());
            var w = (int)GameSettings.SurfaceWidth;
            var h = (int)GameSettings.SurfaceHeight;
            var size = w * h * 4;
            var pixels = new byte[size];
            Renderer.ReadPixels(0, 0, w, h, PixelFormat.Rgba, PixelType.Byte, pixels);
            
            for (var i = 0; i < size; i += 4)
            {
                var a = pixels[i + 3] * 2 + 1;
                var b = pixels[i + 2] * 2 + 1;
                var g = pixels[i + 1] * 2 + 1;
                var r = pixels[i + 0] * 2 + 1;

                pixels[i + 0] = (byte)r;
                pixels[i + 1] = (byte)g;
                pixels[i + 2] = (byte)b;
                pixels[i + 3] = (byte)a;
            }
            
            using var bmp = Image.LoadPixelData<Rgba32>(pixels, w, h);
            bmp.Mutate(X => X.Flip(FlipMode.Vertical));
            var name = DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss") + ".png";
            bmp.SaveAsPngAsync(Path + name);
            return name;
        }
        
        private static double LinearToSrgb(double c)
        {
            double a = .055;
            if (c <= .0031308)
                return 12.92 * c;
            return (1.0 + a) * Math.Pow(c, 1.0/2.4) - a;
        }

        private static double SrgbToLinear(double c)
        {
            double a = .055;
            if (c <= .04045)
                return c / 12.92;
            return Math.Pow((c+a) / (1+a), 2.4);
        }
    }
}