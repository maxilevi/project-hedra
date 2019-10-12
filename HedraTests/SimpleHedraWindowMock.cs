using System;
using System.ComponentModel;
using System.Drawing;
using Hedra.Engine.Events;
using Hedra.Engine.Loader;
using OpenToolkit.Mathematics;
using OpenTK.Input;
using OpenTK.Platform;

namespace HedraTests
{
    public class SimpleHedraWindowMock : IHedra, IEventProvider
    {
        public double TargetFramerate { get; set; }
        public bool IsExiting => true;
        public VSyncMode VSync { get; set; }
        public WindowState WindowState { get; set; }
        public bool Exists { get; }
        public int Height { get; set; }
        public Icon Icon { get; set; }
        public string Title { get; set; }
        public int Width { get; set; }
        public WindowBorder WindowBorder { get; set; }
        public bool CursorVisible { get; set; }
        
        public void Exit()
        {
        }

        public void Run()
        {
        }

        public void RunOnce()
        {
        }

        public void Dispose()
        {
        }

        public void Close()
        {
        }

        public event EventHandler<KeyboardKeyEventArgs> KeyDown;
        public event EventHandler<KeyPressEventArgs> KeyPress;
        public event EventHandler<KeyboardKeyEventArgs> KeyUp;
        public event EventHandler<MouseButtonEventArgs> MouseDown;
        public event EventHandler<MouseButtonEventArgs> MouseUp;
        public event EventHandler<MouseMoveEventArgs> MouseMove;
        public event EventHandler<MouseWheelEventArgs> MouseWheel;
        public bool FinishedLoadingSplashScreen => true;
        public string GameVersion { get; }
        public event OnFrameChanged FrameChanged;
    }
}