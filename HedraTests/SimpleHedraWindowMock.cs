using System;
using Hedra.Engine.Events;
using Hedra.Engine.Loader;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Common.Input;

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
        public WindowIcon Icon { get; set; }
        public string Title { get; set; }
        public int Width { get; set; }
        public WindowBorder WindowBorder { get; set; }
        public bool CursorVisible { get; set; }

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
        public bool FinishedLoadingSplashScreen => true;
        public string GameVersion { get; }
        public void Setup()
        {
        }

        public Vector2 MousePosition { get; }
        public event OnFrameChanged FrameChanged;
    }
}