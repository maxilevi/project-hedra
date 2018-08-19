
using System;
using OpenTK.Graphics;
using Hedra.Engine.Management;
using System.Collections.Generic;
using OpenTK;
using Hedra.Engine;
using System.Windows.Forms;

namespace Hedra
{
	static class Program
	{
		public static Hedra GameWindow;
		
		[STAThread]
		static void Main(string[] Args)
        {
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
		        GameWindow.Icon = AssetManager.LoadIcon("Assets/Icon.ico");
		    }
		    GameSettings.Width = GameWindow.ClientSize.Width;
		    GameSettings.Height = GameWindow.ClientSize.Height;
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

