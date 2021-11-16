using Silk.NET.Windowing;

namespace Hedra.Engine.Loader
{
    public interface IHedra : IHedraWindow
    {
        bool FinishedLoadingSplashScreen { get; }
        string GameVersion { get; }
        int BuildNumber { get; }
        IWindow Window { get; }
        void Setup();
    }
}