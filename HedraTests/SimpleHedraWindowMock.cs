using System;
using System.ComponentModel;
using System.Drawing;
using Hedra.Engine.Loader;
using OpenTK;
using OpenTK.Input;
using OpenTK.Platform;

namespace HedraTests
{
    public class SimpleHedraWindowMock : IHedra
    {
        public double TargetFramerate { get; set; }
        public bool IsExiting => true;
        public VSyncMode VSync { get; set; }
        public WindowState WindowState { get; set; }
        public Rectangle Bounds { get; set; }
        public Rectangle ClientRectangle { get; set; }
        public Size ClientSize { get; set; }
        public MouseCursor Cursor { get; set; }
        public bool Exists { get; }
        public bool Focused { get; }
        public int Height { get; set; }
        public Icon Icon { get; set; }
        public Point Location { get; set; }
        public Size Size { get; set; }
        public string Title { get; set; }
        public bool Visible { get; set; }
        public int Width { get; set; }
        public WindowBorder WindowBorder { get; set; }
        public IWindowInfo WindowInfo { get; }
        public int X { get; set; }
        public int Y { get; set; }
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

        public Point PointToClient(Point point)
        {
            return default(Point);
        }

        public Point PointToScreen(Point point)
        {
            return default(Point);
        }

        public void ProcessEvents()
        {
        }

        public event EventHandler<EventArgs> Closed;
        public event EventHandler<CancelEventArgs> Closing;
        public event EventHandler<EventArgs> Disposed;
        public event EventHandler<EventArgs> FocusedChanged;
        public event EventHandler<EventArgs> IconChanged;
        public event EventHandler<KeyboardKeyEventArgs> KeyDown;
        public event EventHandler<KeyPressEventArgs> KeyPress;
        public event EventHandler<KeyboardKeyEventArgs> KeyUp;
        public event EventHandler<EventArgs> Move;
        public event EventHandler<EventArgs> MouseEnter;
        public event EventHandler<EventArgs> MouseLeave;
        public event EventHandler<EventArgs> Resize;
        public event EventHandler<EventArgs> TitleChanged;
        public event EventHandler<EventArgs> VisibleChanged;
        public event EventHandler<EventArgs> WindowBorderChanged;
        public event EventHandler<EventArgs> WindowStateChanged;
        public event EventHandler<MouseButtonEventArgs> MouseDown;
        public event EventHandler<MouseButtonEventArgs> MouseUp;
        public event EventHandler<MouseMoveEventArgs> MouseMove;
        public event EventHandler<MouseWheelEventArgs> MouseWheel;
        public event EventHandler<FileDropEventArgs> FileDrop;
        public DebugInfoProvider DebugProvider { get; }
        public SplashScreen SplashScreen { get; }
        public string GameVersion { get; }
    }
}