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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
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
            var pixels = new int[w * h];
            Renderer.ReadPixels(0, 0, w, h, PixelFormat.Rgba, PixelType.Byte, pixels);

            // we need to process the pixels a bit to deal with the format difference between OpenGL and .NET
            var data = new byte[w * h * 3];
            for (var i = 0; i < pixels.Length; i++)
            {
                var p = pixels[i];
                var r = p & 0xff;
                var g = (p >> 8) & 0xff;
                var b = (p >> 16) & 0xff;
                data[i+0] = (byte)r;
                data[i+1] = (byte)g;
                data[i+2] = (byte)b;
            }

            using var bmp = Image.LoadPixelData<Rgb24>(data, w, h);
            var name = DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss") + ".png";
            bmp.SaveAsPngAsync(Path + name);
            return name;
        }
    }
}