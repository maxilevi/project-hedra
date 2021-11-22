using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using Hedra.Engine.Events;
using Hedra.Engine.Windowing;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using HedraCursor = Hedra.Input.Cursor;

namespace Hedra.Engine.Loader
{
    public abstract class HedraWindow : IEventProvider, IHedraWindow
    {
        private readonly Stopwatch _watch;

        private bool _cursorVisible;
        private bool _fullscreen;
        private Vector2 _mousePosition;
        private IMouse _mouse;

        protected HedraWindow(int Width, int Height, IMonitor Monitor, ContextProfile Profile, ContextFlags Flags,
            APIVersion Version)
        {
            _watch = new Stopwatch();
            var options = new WindowOptions
            {
                API = new GraphicsAPI(ContextAPI.OpenGL, Profile, Flags, Version),
                Size = new Vector2D<int>(Width, Height),
                ShouldSwapAutomatically = false,
                IsVisible = true,
                Title = "Project Hedra",
                VSync = false,
            };
            Window = Monitor.CreateWindow(options);
            Window.Load += Load;
            Window.Render += RenderFrame;
            Window.Update += UpdateFrame;
            Window.Resize += Resize;
            Window.FocusChanged += FocusChanged;
            Window.Closing += Unload;
            Window.Initialize();

            var input = Window.CreateInput();
            var keyboard = input.Keyboards[0];
            keyboard.KeyDown += ProcessKeyDown;
            input.Keyboards[0].KeyUp += ProcessKeyUp;

            _mouse = input.Mice[0];
            _mouse.MouseMove += (_, Point) =>
                MouseMove?.Invoke(new MouseMoveEventArgs(_mousePosition = new Vector2(Point.X, Point.Y)));
            _mouse.MouseDown += (_, Button) =>
                MouseDown?.Invoke(new MouseButtonEventArgs(Button, InputAction.Press, _mousePosition));
            _mouse.MouseUp += (_, Button) =>
                MouseUp?.Invoke(new MouseButtonEventArgs(Button, InputAction.Release, _mousePosition));
            _mouse.Scroll += (_, Wheel) => MouseWheel?.Invoke(new MouseWheelEventArgs(Wheel.X, Wheel.Y));
            HedraCursor.Mouse = _mouse;

            unsafe
            {
                GlfwProvider.GLFW.Value.SetCharCallback(
                    (WindowHandle*)Window.Handle,
                    (_, Codepoint) => CharWritten?.Invoke(char.ConvertFromUtf32((int)Codepoint))
                );
            }
        }

        public IWindow Window { get; }

        public event Action<MouseButtonEventArgs> MouseUp;
        public event Action<MouseButtonEventArgs> MouseDown;
        public event Action<MouseWheelEventArgs> MouseWheel;
        public event Action<MouseMoveEventArgs> MouseMove;
        public event Action<KeyboardKeyEventArgs> KeyDown;
        public event Action<KeyboardKeyEventArgs> KeyUp;
        public event Action<string> CharWritten;

        public void Run()
        {
            Window.IsVisible = true;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            Load();
            Window.Run();
        }

        public int Width
        {
            get => Window.Size.X;
            set => Window.Size = new Vector2D<int>(value, Window.Size.Y);
        }

        public int Height
        {
            get => Window.Size.Y;
            set => Window.Size = new Vector2D<int>(Window.Size.X, value);
        }

        public double TargetFramerate { get; set; }
        /*{
            get => Window.FramesPerSecond;
            set => Window.FramesPerSecond = value;
        }*/

        public bool IsExiting => Window.Handle == IntPtr.Zero || Window.IsClosing;

        public bool VSync
        {
            get => Window.VSync;
            set => Window.VSync = value;
        }

        public WindowState WindowState
        {
            get => Window.WindowState;
            set => Window.WindowState = value;
        }

        public bool Exists => !IsExiting;

        public string Title
        {
            get => Window.Title;
            set => Window.Title = value;
        }

        public void Close()
        {
            Window.Close();
        }

        public bool Fullscreen
        {
            get => _fullscreen;
            set
            {
                _fullscreen = value;
                Window.WindowState = _fullscreen ? WindowState.Fullscreen : WindowState.Normal;
            }
        }

        public bool CursorVisible
        {
            get => _cursorVisible;
            set
            {
                _cursorVisible = value;
                _mouse.Cursor.CursorMode = _cursorVisible ? CursorMode.Normal : CursorMode.Hidden;
            }
        }

        protected abstract void RenderFrame(double Delta);
        protected abstract void UpdateFrame(double Delta);
        protected abstract void Unload();
        protected abstract void FocusChanged(bool IsFocused);
        protected abstract void Load();
        protected abstract void Resize(Vector2D<int> Size);

        private void ProcessKeyDown(IKeyboard Keyboard, Key Key, int Mods)
        {
            KeyDown?.Invoke(new KeyboardKeyEventArgs(Key, (KeyModifiers)Mods));
        }

        private void ProcessKeyUp(IKeyboard Keyboard, Key Key, int Mods)
        {
            KeyUp?.Invoke(new KeyboardKeyEventArgs(Key, (KeyModifiers)Mods));
        }

        public void Dispose()
        {
            Window.Close();
        }
    }
}