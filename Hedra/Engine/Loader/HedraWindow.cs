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
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing;
using Image = Silk.NET.GLFW.Image;
using Monitor = Silk.NET.Windowing.Monitor;

namespace Hedra.Engine.Loader
{
    public abstract class HedraWindow : IEventProvider, IHedraWindow
    {
        public IWindow Window { get; }
        
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
                Size = new Vector2D<int>(Width, Height),
                ShouldSwapAutomatically = true,
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
            
            var mouse = input.Mice[0];
            mouse.MouseMove += (_, Point) => MouseMove?.Invoke(new MouseMoveEventArgs(_mousePosition = new Vector2(Point.X, Point.Y)));
            mouse.MouseDown += (_, Button) => MouseDown?.Invoke(new MouseButtonEventArgs(Button, InputAction.Press, _mousePosition));
            mouse.MouseUp += (_, Button) => MouseUp?.Invoke(new MouseButtonEventArgs(Button, InputAction.Release, _mousePosition));
            mouse.Scroll += (_, Wheel) => MouseWheel?.Invoke(new MouseWheelEventArgs(Wheel.X, Wheel.Y));
            
            unsafe
            {
                GlfwProvider.GLFW.Value.SetCharCallback(
                    (WindowHandle*)Window.Handle,
                    (_, Codepoint) => CharWritten?.Invoke(char.ConvertFromUtf32((int)Codepoint))
                );
            }
        }

        protected abstract void RenderFrame(double Delta);
        protected abstract void UpdateFrame(double Delta);
        protected abstract void Unload();
        protected abstract void FocusChanged(bool IsFocused);
        protected abstract void Load();
        protected abstract void Resize(Vector2D<int> Size);
        
        public void Run()
        {
            Window.IsVisible = true;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            Load();
            Window.Run();
        }

        private void ProcessKeyDown(IKeyboard Keyboard, Key Key, int Mods)
        {
            KeyDown?.Invoke(new KeyboardKeyEventArgs(Key, (KeyModifiers) Mods));
        }

        private void ProcessKeyUp(IKeyboard Keyboard, Key Key, int Mods)
        {
            KeyUp?.Invoke(new KeyboardKeyEventArgs(Key, (KeyModifiers) Mods));
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

        public void Dispose()
        {
            Window.Close();
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
                unsafe
                {
                    var glfw = GlfwProvider.GLFW.Value;
                    var monitor = glfw.GetPrimaryMonitor();
                    var mode = glfw.GetVideoMode(monitor);
                    glfw.SetWindowMonitor
                    (
                        (WindowHandle*)Window.Handle,
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
                    glfw.SetInputMode((WindowHandle*)Window.Handle, CursorStateAttribute.Cursor, mode);
                }
            }
        }

        public void SetIcon(Image Icon)
        {
            unsafe
            {
                var glfw = GlfwProvider.GLFW.Value;
                glfw.SetWindowIcon((WindowHandle*) Window.Handle, 1, &Icon);
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