using System;
using System.Drawing;
using Hedra.Engine.Events;
using Hedra.Engine.Windowing;
using OpenToolkit.Mathematics;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;

namespace Hedra.Engine.Loader
{
    public abstract class HedraWindow : IEventProvider, IHedraWindow
    {
        private readonly IWindow _window;

        protected HedraWindow(int Width, int Height, ContextProfile Profile, ContextFlags Flags, APIVersion Version) : base()
        {
            var options = new WindowOptions
            {
                API = new GraphicsAPI(ContextAPI.OpenGL, Profile, Flags, Version),
                Size = new Size(Width, Height),
                ShouldSwapAutomatically = true,
                IsVisible = true,
                Title = "Project Hedra",
                UseSingleThreadedWindow = true,
                WindowState = WindowState.Maximized
            };
            _window = Window.Create(options);
            _window.Load += Load;
            _window.Render += RenderFrame;
            _window.Update += UpdateFrame;
            _window.Resize += Resize;
            _window.FocusChanged += FocusChanged;
            _window.Closing += Unload;
            _window.Open();
            
            var input = _window.GetInput();
            var keyboard = input.Keyboards[0];
            keyboard.KeyDown += (Keyboard, Key) => KeyDown?.Invoke(new KeyboardKeyEventArgs(Key, default));
            input.Keyboards[0].KeyUp += (Keyboard, Key) => KeyUp?.Invoke(new KeyboardKeyEventArgs(Key, default));
            var mouse = input.Mice[0];
            mouse.MouseMove += (_, Point) => MouseMove?.Invoke(new MouseMoveEventArgs(new Vector2(Point.X, Point.Y)));
            mouse.MouseDown += (_, Button) => MouseDown?.Invoke(new MouseButtonEventArgs(Button, InputAction.Press));
            mouse.MouseUp += (_, Button) => MouseUp?.Invoke(new MouseButtonEventArgs(Button, InputAction.Release));
            mouse.Scroll += (_, Wheel) => MouseWheel?.Invoke(new MouseWheelEventArgs(Wheel.X, Wheel.Y));
        }

        protected abstract void RenderFrame(double Delta);
        protected abstract void UpdateFrame(double Delta);
        protected abstract void Unload();
        protected abstract void FocusChanged(bool IsFocused);
        protected abstract void Load();
        protected abstract void Resize(Size Size);

        public int Width
        {
            get => _window.Size.Width;
            set => _window.Size = new Size(value, _window.Size.Height);
        }
        public int Height
        {
            get => _window.Size.Height;
            set => _window.Size = new Size(_window.Size.Width, value);
        }

        public Vector2 MousePosition => new Vector2(_window.GetInput().Mice[0].Position.X, _window.GetInput().Mice[0].Position.Y);

        public double TargetFramerate
        {
            get => _window.FramesPerSecond;
            set => _window.FramesPerSecond = 1f / value;
        }

        public bool IsExiting => _window.IsClosing;

        public VSyncMode VSync
        {
            get => _window.VSync;
            set => _window.VSync = value;
        }

        public WindowState WindowState
        {
            get => _window.WindowState;
            set => _window.WindowState = value;
        }

        public bool Exists => !IsExiting;

        public string Title
        {
            get => _window.Title;
            set => _window.Title = value;
        }
        
        public WindowBorder WindowBorder
        {
            get => _window.WindowBorder;
            set => _window.WindowBorder = value;
        }
        
        public void Run()
        {
            while (!_window.IsClosing)
            {
                _window.DoEvents();
                _window.DoUpdate();
                _window.DoRender();
            }
            _window.Reset();
        }

        public void Dispose()
        {
            _window.Close();
        }

        public void Close()
        {
            _window.Close();
        }

        public bool CursorVisible
        {
            get => _window.GetInput().Mice[0].IsVisible;
            set => _window.GetInput().Mice[0].IsVisible = value;
        }
        
        public event Action<MouseButtonEventArgs> MouseUp;
        public event Action<MouseButtonEventArgs> MouseDown;
        public event Action<MouseWheelEventArgs> MouseWheel;
        public event Action<MouseMoveEventArgs> MouseMove;
        public event Action<KeyboardKeyEventArgs> KeyDown;
        public event Action<KeyboardKeyEventArgs> KeyUp;
    }
}