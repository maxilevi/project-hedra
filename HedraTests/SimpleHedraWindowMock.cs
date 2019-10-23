using System;
using Hedra.Engine.Events;
using Hedra.Engine.Loader;
using Hedra.Engine.Windowing;
using System.Numerics;
using Silk.NET.GLFW;
using Silk.NET.Windowing.Common;


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
        public string Title { get; set; }
        public int Width { get; set; }
        public WindowBorder WindowBorder { get; set; }
        public bool CursorVisible { get; set; }
        public bool Fullscreen { get; set; }

        public void Run()
        {
        }
        public void Dispose()
        {
        }

        public void Close()
        {
        }

        public event Action<KeyboardKeyEventArgs> KeyDown;
        public event Action<KeyboardKeyEventArgs> KeyUp;
        public event Action<MouseButtonEventArgs> MouseDown;
        public event Action<MouseButtonEventArgs> MouseUp;
        public event Action<MouseMoveEventArgs> MouseMove;
        public event Action<MouseWheelEventArgs> MouseWheel;
        public event Action<string> CharWritten;
        public bool FinishedLoadingSplashScreen => true;
        public string GameVersion { get; }
        public void Setup()
        {
        }

        public Vector2 MousePosition { get; }
        public void SetIcon(Image Icon)
        {
        }

        public event OnFrameChanged FrameChanged;
    }
}