
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

		        Constants.DEVICE_WIDTH = Screen.PrimaryScreen.Bounds.Width;
		        Constants.DEVICE_HEIGHT = Screen.PrimaryScreen.Bounds.Height;


		        Log.WriteLine("Creating the window on the Primary Device at " + Constants.DEVICE_WIDTH + "x" +
		                      Constants.DEVICE_HEIGHT);

		        Constants.WIDTH = Constants.DEVICE_WIDTH;
		        Constants.HEIGHT = Constants.DEVICE_HEIGHT;
		        Constants.SCREEN_RATIO = Constants.WIDTH / (float) Constants.HEIGHT;

		        GameWindow = new Hedra(Constants.WIDTH, Constants.HEIGHT,
		            GraphicsMode.Default, "Project Hedra");

		        GameWindow.Icon = AssetManager.LoadIcon("Assets/Icon.ico");
		        GameWindow.WindowState = WindowState.Maximized;
		        Constants.WIDTH = GameWindow.ClientSize.Width;
		        Constants.HEIGHT = GameWindow.ClientSize.Height;
		        Constants.SCREEN_RATIO = Constants.WIDTH / (float) Constants.HEIGHT;
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

