using System;
using System.ComponentModel;
using System.Drawing;
using OpenTK;
using OpenTK.Input;
using OpenTK.Platform;

namespace Hedra.Engine.Loader
{
    public interface IHedraWindow
    {
        double TargetFramerate { get; set; }
        bool IsExiting { get; }
        VSyncMode VSync { get; set; }
        WindowState WindowState { get; set; }
        Rectangle Bounds { get; set; }
        Rectangle ClientRectangle { get; set; }
        Size ClientSize { get; set; }
        MouseCursor Cursor { get; set; }
        bool Exists { get; }
        bool Focused { get; }
        int Height { get; set; }
        Icon Icon { get; set; }
        Point Location { get; set; }
        Size Size { get; set; }
        string Title { get; set; }
        bool Visible { get; set; }
        int Width { get; set; }
        WindowBorder WindowBorder { get; set; }
        IWindowInfo WindowInfo { get; }
        int X { get; set; }
        int Y { get; set; }
        bool CursorVisible { get; set; }
        void Exit();
        void Run();
        void Dispose();
        void Close();
        Point PointToClient(Point point);
        Point PointToScreen(Point point);
        void ProcessEvents();
        event EventHandler<EventArgs> Closed;
        event EventHandler<CancelEventArgs> Closing;
        event EventHandler<EventArgs> Disposed;
        event EventHandler<EventArgs> FocusedChanged;
        event EventHandler<EventArgs> IconChanged;
        event EventHandler<KeyboardKeyEventArgs> KeyDown;
        event EventHandler<KeyPressEventArgs> KeyPress;
        event EventHandler<KeyboardKeyEventArgs> KeyUp;
        event EventHandler<EventArgs> Move;
        event EventHandler<EventArgs> MouseEnter;
        event EventHandler<EventArgs> MouseLeave;
        event EventHandler<EventArgs> Resize;
        event EventHandler<EventArgs> TitleChanged;
        event EventHandler<EventArgs> VisibleChanged;
        event EventHandler<EventArgs> WindowBorderChanged;
        event EventHandler<EventArgs> WindowStateChanged;
        event EventHandler<MouseButtonEventArgs> MouseDown;
        event EventHandler<MouseButtonEventArgs> MouseUp;
        event EventHandler<MouseMoveEventArgs> MouseMove;
        event EventHandler<MouseWheelEventArgs> MouseWheel;
        event EventHandler<FileDropEventArgs> FileDrop;
    }
}