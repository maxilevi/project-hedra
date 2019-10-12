using Hedra.Engine.Events;
using OpenToolkit.Mathematics;
using OpenTK.Platform;

namespace Hedra.Engine.Loader
{
    public interface IHedra : IHedraWindow
    {
        bool FinishedLoadingSplashScreen { get; }
        string GameVersion { get; }
        event OnFrameChanged FrameChanged;
    }
}