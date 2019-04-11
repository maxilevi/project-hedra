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
        bool Exists { get; }
        int Height { get; set; }
        Icon Icon { get; set; }
        string Title { get; set; }
        int Width { get; set; }
        WindowBorder WindowBorder { get; set; }
        bool CursorVisible { get; set; }
        void Exit();
        void Run();
        void RunOnce();
        void Dispose();
        void Close();
    }
}