using System;
using System.Drawing;
using System.Threading;
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
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

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
        public Icon Icon { get; set; }
        public string Title { get; set; }
        public int Width { get; set; }
        public WindowBorder WindowBorder { get; set; }
        public bool CursorVisible { get; set; }
        
        public bool FinishedLoadingSplashScreen => true;
        public string GameVersion => "SERVER";
        public event OnFrameChanged FrameChanged;
        public event EventHandler<MouseButtonEventArgs> MouseUp;
        public event EventHandler<MouseButtonEventArgs> MouseDown;
        public event EventHandler<MouseWheelEventArgs> MouseWheel;
        public event EventHandler<MouseMoveEventArgs> MouseMove;
        public event EventHandler<KeyboardKeyEventArgs> KeyDown;
        public event EventHandler<KeyboardKeyEventArgs> KeyUp;
        public event EventHandler<KeyPressEventArgs> KeyPress;
    }
}