using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using Hedra.Engine.Events;
using Hedra.Engine.Windowing;
using System.Numerics;
using Hedra.Engine.Core;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Input.Common;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using Image = Silk.NET.GLFW.Image;
using Monitor = Silk.NET.Windowing.Monitor;

namespace Hedra.Engine.Loader
{
    public abstract class HedraWindow : IEventProvider, IHedraWindow
    {
        private readonly IWindow _window;
        private bool _cursorVisible;
        private bool _fullscreen;
        private readonly Stopwatch _watch;
        private SpinWait _spinner;
        private Vector2 _mousePosition;

        protected HedraWindow(int Width, int Height, IMonitor Monitor, ContextProfile Profile, ContextFlags Flags, APIVersion Version) : base()
        {
            _watch = new Stopwatch();
            _spinner = new SpinWait();
            var options = new WindowOptions
            {
                API = new GraphicsAPI(ContextAPI.OpenGL, Profile, Flags, Version),
                Size = Monitor.VideoMode.Resolution ?? Monitor.Bounds.Size,
                ShouldSwapAutomatically = true,
                IsVisible = true,
                Title = "Project Hedra",
                UseSingleThreadedWindow = true,
            };
            _window = Monitor.CreateWindow(options);
            _window.Load += Load;
            _window.Render += RenderFrame;
            _window.Update += UpdateFrame;
            _window.Resize += Resize;
            _window.FocusChanged += FocusChanged;
            _window.Closing += Unload;
            _window.Initialize();

            var input = _window.CreateInput();
            var keyboard = input.Keyboards[0];
            keyboard.KeyDown += ProcessKeyDown;
            input.Keyboards[0].KeyUp += ProcessKeyUp;
            
            var mouse = input.Mice[0];
            mouse.MouseMove += (_, Point) => MouseMove?.Invoke(new MouseMoveEventArgs(_mousePosition = new Vector2(Point.X, Point.Y)));
            mouse.MouseDown += (_, Button) => MouseDown?.Invoke(new MouseButtonEventArgs(Button, InputAction.Press, _mousePosition));
            mouse.MouseUp += (_, Button) => MouseUp?.Invoke(new MouseButtonEventArgs(Button, InputAction.Release, _mousePosition));
            mouse.Scroll += (_, Wheel) => MouseWheel?.Invoke(new MouseWheelEventArgs(Wheel.X, Wheel.Y));
            
            unsafe
            {
                GlfwProvider.GLFW.Value.SetCharCallback(
                    (WindowHandle*)_window.Handle,
                    (_, Codepoint) => CharWritten?.Invoke(char.ConvertFromUtf32((int)Codepoint))
                );
            }
        }

        protected abstract void RenderFrame(double Delta);
        protected abstract void UpdateFrame(double Delta);
        protected abstract void Unload();
        protected abstract void FocusChanged(bool IsFocused);
        protected abstract void Load();
        protected abstract void Resize(Size Size);
        
        public void Run()
        {
            _window.IsVisible = true;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            Load();
            Resize(new Size(Width, Height));
            _watch.Start();
            var lastTick = _watch.Elapsed.TotalSeconds;
            var elapsed = .0;
            var frames = 0;
            while (!_window.IsClosing)
            {
                _window.DoEvents();
                if (_window.IsClosing) continue;
                
                var totalSeconds = _watch.Elapsed.TotalSeconds;
                var time = Math.Min(1.0, totalSeconds - lastTick);
                lastTick = totalSeconds;
                UpdateFrame(time);
                RenderFrame(time);
                _window.SwapBuffers();
                while (_watch.Elapsed.TotalSeconds - lastTick < TargetFramerate)
                {
                    _spinner.SpinOnce();
                }
            }
            _window.Reset();
        }

        private void ProcessKeyDown(IKeyboard Keyboard, Key Key, int ScanCode, int Mods)
        {
            KeyDown?.Invoke(new KeyboardKeyEventArgs(Key, (KeyModifiers) Mods));
        }

        private void ProcessKeyUp(IKeyboard Keyboard, Key Key, int ScanCode, int Mods)
        {
            KeyUp?.Invoke(new KeyboardKeyEventArgs(Key, (KeyModifiers) Mods));
        }

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
        
        public double TargetFramerate { get; set; }

        public bool IsExiting => _window.Handle == IntPtr.Zero || _window.IsClosing;

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

        public void Dispose()
        {
            _window.Close();
        }

        public void Close()
        {
            _window.Close();
        }

        public bool Fullscreen
        {
            get => _fullscreen;
            set
            {
                _fullscreen = value;
                unsafe
                {
                    var glfw = GlfwProvider.GLFW.Value;
                    var monitor = glfw.GetPrimaryMonitor();
                    var mode = glfw.GetVideoMode(monitor);
                    glfw.SetWindowMonitor
                    (
                        (WindowHandle*)_window.Handle,
                        _fullscreen ? monitor : null, 0, 0, mode->Width, mode->Height,
                        mode->RefreshRate
                    );
                }
            }
        }

        public bool CursorVisible
        {
            get => _cursorVisible;
            set
            {
                unsafe
                {
                    _cursorVisible = value;
                    var glfw = GlfwProvider.GLFW.Value;
                    var mode = _cursorVisible ? CursorModeValue.CursorNormal : CursorModeValue.CursorHidden;
                    glfw.SetInputMode((WindowHandle*)_window.Handle, CursorStateAttribute.Cursor, mode);
                }
            }
        }

        public void SetIcon(Image Icon)
        {
            unsafe
            {
                var glfw = GlfwProvider.GLFW.Value;
                glfw.SetWindowIcon((WindowHandle*) _window.Handle, 1, &Icon);
            }
        }

        public event Action<MouseButtonEventArgs> MouseUp;
        public event Action<MouseButtonEventArgs> MouseDown;
        public event Action<MouseWheelEventArgs> MouseWheel;
        public event Action<MouseMoveEventArgs> MouseMove;
        public event Action<KeyboardKeyEventArgs> KeyDown;
        public event Action<KeyboardKeyEventArgs> KeyUp;
        public event Action<string> CharWritten;
    }
}