
using System;
using OpenTK.Graphics;
using Hedra.Engine.Management;
using System.Collections.Generic;
using OpenTK;
using Hedra.Engine;
using System.Windows.Forms;
using System.Drawing;
using Hedra.Engine.Rendering;
using OpenTK.Graphics.OpenGL4;

namespace Hedra
{
	static class Program
	{
		public static bool IsDebug { get; private set; }
		public static bool IsRelease => !IsDebug;
		public static Hedra GameWindow;
		
		static void Main(string[] Args)
        {
	        #if DEBUG
	        IsDebug = true;
	        #endif
	        
            var devices = new List<DisplayDevice>();
		    for (var index = 0; index < 6; ++index)
		    {
		        var display = DisplayDevice.GetDisplay((DisplayIndex)(0 + index));
		        if (display != null)
		            devices.Add(display);
		    }
            var device = DisplayDevice.Default;
		    Log.WriteLine("Available Devices: " + Environment.NewLine);
		    for (var i = 0; i < devices.Count; i++)
		    {
		        if (devices[i].Width > device.Width && devices[i].Height > device.Height)
		            device = devices[i];
		        Log.WriteLine(devices[i].Bounds.ToString());
		    }

		    GameSettings.DeviceWidth = Screen.PrimaryScreen.Bounds.Width;
		    GameSettings.DeviceHeight = Screen.PrimaryScreen.Bounds.Height;


		    Log.WriteLine("Creating the window on the Primary Device at " + GameSettings.DeviceWidth + "x" +
		                    GameSettings.DeviceHeight);

		    GameSettings.Width = GameSettings.DeviceWidth;
		    GameSettings.Height = GameSettings.DeviceHeight;
		    GameSettings.ScreenRatio = GameSettings.Width / (float) GameSettings.Height;

		    GameWindow = new Hedra(GameSettings.Width, GameSettings.Height,
		        GraphicsMode.Default, "Project Hedra", device, 3, 3);
		    GameWindow.WindowState = WindowState.Maximized;
		    if (OSManager.RunningPlatform == Platform.Windows)
		    {
		        //GameWindow.Icon = AssetManager.LoadIcon("Assets/Icon.ico");
		    }

	        GameSettings.SurfaceWidth = GameWindow.Width;
	        GameSettings.SurfaceHeight = GameWindow.Height;

	        GameSettings.ScreenRatio = GameSettings.Width / (float) GameSettings.Height;	
#if DEBUG
            GameWindow.Run(60, 60);
#else
            try
	        {
		        GameWindow.Run(60, 60);
	        }
	        catch (Exception e)
	        {
		        Log.WriteLine(e);
		        throw;
	        }
#endif
        }
    }
}

