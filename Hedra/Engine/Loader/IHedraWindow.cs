using Silk.NET.Windowing;

namespace Hedra.Engine.Loader
{
    public interface IHedraWindow
    {
        double TargetFramerate { get; set; }
        bool IsExiting { get; }
        bool VSync { get; set; }
        WindowState WindowState { get; set; }
        bool Exists { get; }
        string Title { get; set; }
        bool CursorVisible { get; set; }
        bool Fullscreen { get; set; }
        int Width { get; set; }
        int Height { get; set; }
        void Run();
        void Close();
    }
}