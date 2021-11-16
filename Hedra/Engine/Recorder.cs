/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 21/06/2016
 * Time: 11:06 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Hedra.Core;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Steamworks;
using Hedra.Engine.Windowing;
using Hedra.Game;
using SixLabors.ImageSharp;
using PixelFormat = Hedra.Engine.Windowing.PixelFormat;

namespace Hedra.Engine
{
    /// <summary>
    ///     Description of Recorder.
    /// </summary>
    public static class Recorder
    {
        public static bool Active;
        public static string Output = "C:/Recordings/" + DateTime.Now.TimeOfDay.Minutes + "/";

        public static long FrameID;

        public static void Record()
        {
            if (!Active)
                return;

            Directory.CreateDirectory(Output);
            var w = (int)GameSettings.SurfaceWidth;
            var h = (int)GameSettings.SurfaceHeight;
            var pixels = new int[w * h];
            Renderer.ReadPixels(0, 0, w, h, PixelFormat.Rgba, PixelType.Byte, pixels);

            TaskScheduler.Asynchronous(delegate
            {
                // we need to process the pixels a bit to deal with the format difference between OpenGL and .NET
                for (var i = 0; i < pixels.Length; i++)
                {
                    var p = pixels[i];
                    var r = p & 0xff;
                    var g = (p >> 8) & 0xff;
                    var b = (p >> 16) & 0xff;
                    pixels[i] = ((r << 16) | (g << 8) | b) << 1;
                }

                Bitmap Bmp = new Bitmap(w, h);
                var data = Bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
                Bmp.UnlockBits(data);


                Bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

                Bmp.Save(Output + FrameID++ + ".png", ImageFormat.Png);
            });
        }

        public static string SaveScreenshot(string Path)
        {
            Steam.Instance.CallIf(S => S.Screenshots.Trigger());
            var w = (int)GameSettings.SurfaceWidth;
            var h = (int)GameSettings.SurfaceHeight;
            var pixels = new int[w * h];
            Renderer.ReadPixels(0, 0, w, h, PixelFormat.Rgba, PixelType.Byte, pixels);

            // we need to process the pixels a bit to deal with the format difference between OpenGL and .NET
            for (var i = 0; i < pixels.Length; i++)
            {
                var p = pixels[i];
                var r = p & 0xff;
                var g = (p >> 8) & 0xff;
                var b = (p >> 16) & 0xff;
                pixels[i] = ((r << 16) | (g << 8) | b) << 1;
            }

            using (var Bmp = new Bitmap(w, h))
            {
                var data = Bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
                Bmp.UnlockBits(data);
                Bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Bmp.Save(Path + DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss") + ".png",
                    ImageFormat.Png);
                return DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss") + ".png";
            }
        }
    }
}