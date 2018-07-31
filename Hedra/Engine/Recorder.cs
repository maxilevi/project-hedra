/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 21/06/2016
 * Time: 11:06 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Management;

namespace Hedra.Engine
{
	/// <summary>
	/// Description of Recorder.
	/// </summary>
	public static class Recorder
	{
		public static bool Active;
		public static string Output = "C:/Recordings/"+DateTime.Now.TimeOfDay.Minutes+"/";
		
		public static long FrameID;
		public static void Record(){
			
			if(!Active)
				return;
			
			Directory.CreateDirectory(Output);
				
			int[] pixels = new int[GameSettings.Width * GameSettings.Height];
			GL.ReadPixels(0, 0, GameSettings.Width, GameSettings.Height, PixelFormat.Rgba, PixelType.Byte, pixels);
			 
			TaskManager.Asynchronous( delegate{
			// we need to process the pixels a bit to deal with the format difference between OpenGL and .NET
			for (int i = 0; i < pixels.Length; i++)
			{
			    int p = pixels[i];
			    int r = p & 0xff;
			    int g = (p >> 8) & 0xff;
			    int b = (p >> 16) & 0xff;
			    pixels[i] = (r << 16 | g << 8 | b) << 1;
			}
			 
			Bitmap Bmp = new Bitmap(GameSettings.Width, GameSettings.Height);
			var data = Bmp.LockBits(new Rectangle(0, 0, GameSettings.Width, GameSettings.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
			Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
			Bmp.UnlockBits(data);

			
			Bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

			Bmp.Save(Output + FrameID++ + ".png", System.Drawing.Imaging.ImageFormat.Png);
			
			});
		}
		
		public static string SaveScreenshot(string Path){
			int[] pixels = new int[GameSettings.Width * GameSettings.Height];
			GL.ReadPixels(0, 0, GameSettings.Width, GameSettings.Height, PixelFormat.Rgba, PixelType.Byte, pixels);
			 
			// we need to process the pixels a bit to deal with the format difference between OpenGL and .NET
			for (int i = 0; i < pixels.Length; i++)
			{
			    int p = pixels[i];
			    int r = p & 0xff;
			    int g = (p >> 8) & 0xff;
			    int b = (p >> 16) & 0xff;
			    pixels[i] = (r << 16 | g << 8 | b) << 1;
			}

            using (Bitmap Bmp = new Bitmap(GameSettings.Width, GameSettings.Height))
            {
                var data = Bmp.LockBits(new Rectangle(0, 0, GameSettings.Width, GameSettings.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
                Bmp.UnlockBits(data);


                Bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

                Bmp.Save(Path + DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss") + ".png", System.Drawing.Imaging.ImageFormat.Png);
                return DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss") + ".png";
            }
		}
	}
}
