using System.Numerics;

namespace Hedra.Engine.Loader
{
    public interface IHedra : IHedraWindow
    {
        bool FinishedLoadingSplashScreen { get; }
        string GameVersion { get; }
        int BuildNumber { get; }
        void Setup();
    }
}