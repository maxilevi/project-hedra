using OpenToolkit.Mathematics;

namespace Hedra.Engine.Loader
{
    public interface IHedra : IHedraWindow
    {
        bool FinishedLoadingSplashScreen { get; }
        string GameVersion { get; }
        Vector2 MousePosition { get; }
    }
}