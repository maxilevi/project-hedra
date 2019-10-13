using System.Drawing;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Common.Input;

namespace Hedra.Engine.Loader
{
    public interface IHedraWindow
    {
        double TargetFramerate { get; set; }
        bool IsExiting { get; }
        VSyncMode VSync { get; set; }
        WindowState WindowState { get; set; }
        bool Exists { get; }
        WindowIcon Icon { get; set; }
        string Title { get; set; }
        WindowBorder WindowBorder { get; set; }
        bool CursorVisible { get; set; }
        void Run();
        void Dispose();
        void Close();
        int Width { get; set; }
        int Height { get; set; }
    }
}