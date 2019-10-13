using System.Drawing;
using OpenToolkit.Mathematics;
using Silk.NET.Windowing.Common;


namespace Hedra.Engine.Loader
{
    public interface IHedraWindow
    {
        double TargetFramerate { get; set; }
        bool IsExiting { get; }
        VSyncMode VSync { get; set; }
        WindowState WindowState { get; set; }
        bool Exists { get; }
        string Title { get; set; }
        WindowBorder WindowBorder { get; set; }
        void Run();
        bool CursorVisible { get; set; }
        void Close();
        int Width { get; set; }
        int Height { get; set; }
        Vector2 MousePosition { get; }
    }
}