using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Hedra.API;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.Events;
using Hedra.Engine.Game;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.Networking;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Sound;
using Hedra.Game;
using Hedra.Sound;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Common.Input;


namespace Hedra.Engine.Loader
{
    public class HedraServer : IHedra, IEventProvider
    {
        public void Run()
        {
            OnLoad();
            while (IsAlive) OnUpdateFrame();
        }

        private void OnLoad()
        {
            GameSettings.DeviceWidth = 1;
            GameSettings.DeviceHeight = 1;
            GameSettings.Width = GameSettings.DeviceWidth;
            GameSettings.Height = GameSettings.DeviceHeight;

            Renderer.Provider = new DummyGLProvider();
            SoundPlayer.Provider = new DummySoundProvider();
            
            Log.WriteLine("Starting hedra dedicated server...");
            Hedra.LoadBoilerplate();
            var information = LocalPlayer.BuildNewPlayer("HOST", ClassDesign.FromString(Class.Mage));
            GameManager.MakeCurrent(information);
            Network.Instance.Host();
        }

        private void OnUpdateFrame()
        {
            Executer.Update();
            Network.Instance.Update();
            Thread.Sleep(16);
        }

        private bool IsAlive => true;

        public void RunOnce() => throw new System.NotImplementedException();
        public void Dispose() => throw new System.NotImplementedException();
        public void Close() => throw new System.NotImplementedException();
        public void Exit() => throw new System.NotImplementedException();
        public bool IsExiting => false;
        public bool Exists => true;
        
        public double TargetFramerate { get; set; }
        public VSyncMode VSync { get; set; }
        public WindowState WindowState { get; set; }
        public int Height { get; set; }
        public WindowIcon Icon { get; set; }
        public string Title { get; set; }
        public int Width { get; set; }
        public WindowBorder WindowBorder { get; set; }
        public bool CursorVisible { get; set; }
        
        public bool FinishedLoadingSplashScreen => true;
        public string GameVersion => "SERVER";
        public void Setup()
        {
        }

        public Vector2 MousePosition { get; }
        public Vector2 MouseDelta { get; }
        public event OnFrameChanged FrameChanged;
        public event Action<MouseButtonEventArgs> MouseUp;
        public event Action<MouseButtonEventArgs> MouseDown;
        public event Action<MouseWheelEventArgs> MouseWheel;
        public event Action<MouseMoveEventArgs> MouseMove;
        public event Action<KeyboardKeyEventArgs> KeyDown;
        public event Action<KeyboardKeyEventArgs> KeyUp;
        public event Action<KeyPressEventArgs> KeyPress;
    }
}