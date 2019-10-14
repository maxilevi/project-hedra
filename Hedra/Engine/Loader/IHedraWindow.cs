using System.Drawing;
using OpenToolkit.Mathematics;
using Silk.NET.Windowing.Common;
using Image = Silk.NET.GLFW.Image;


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
        void Run();
        bool CursorVisible { get; set; }
        bool Fullscreen { get; set; }
        void Close();
        int Width { get; set; }
        int Height { get; set; }
        Vector2 MousePosition { get; }
        void SetIcon(Image Icon);
    }
}