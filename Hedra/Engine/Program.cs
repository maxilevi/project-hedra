
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
		static void Main(string[] args) {
#if !DEBUG
            try
		    {
#endif
                List<DisplayDevice> Devices = new List<DisplayDevice>();
		        for (int index = 0; index < 6; ++index)
		        {
		            DisplayDevice display = DisplayDevice.GetDisplay((DisplayIndex)(0 + index));
		            if (display != null)
		                Devices.Add(display);
		        }
                DisplayDevice Device = DisplayDevice.Default;
		        Log.WriteLine("Available Devices: " + Environment.NewLine);
		        for (int i = 0; i < Devices.Count; i++)
		        {
		            if (Devices[i].Width > Device.Width && Devices[i].Height > Device.Height)
		                Device = Devices[i];
		            Log.WriteLine(Devices[i].Bounds.ToString());
		        }

		        GameSettings.DeviceWidth = Screen.PrimaryScreen.Bounds.Width;
		        GameSettings.DeviceHeight = Screen.PrimaryScreen.Bounds.Height;


		        Log.WriteLine("Creating the window on the Primary Device at " + GameSettings.DeviceWidth + "x" +
		                      GameSettings.DeviceHeight);

		        GameSettings.Width = GameSettings.DeviceWidth;
		        GameSettings.Height = GameSettings.DeviceHeight;
		        GameSettings.ScreenRatio = GameSettings.Width / (float) GameSettings.Height;

		        GameWindow = new Hedra(GameSettings.Width, GameSettings.Height,
		            GraphicsMode.Default, "Project Hedra");

		        GameWindow.WindowState = WindowState.Maximized;
		        if (OSManager.RunningPlatform == Platform.Windows)
		        {
		            GameWindow.Icon = AssetManager.LoadIcon("Assets/Icon.ico");
		        }
                GameSettings.Width = GameWindow.ClientSize.Width;
		        GameSettings.Height = GameWindow.ClientSize.Height;
		        GameSettings.ScreenRatio = GameSettings.Width / (float) GameSettings.Height;
		        GameWindow.Run();
#if !DEBUG
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

