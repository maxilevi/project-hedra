using Hedra.Engine.Events;
using OpenTK;
using OpenTK.Platform;

namespace Hedra.Engine.Loader
{
    public interface IHedra : IHedraWindow
    {
        DebugInfoProvider DebugProvider { get; }
	    SplashScreen SplashScreen { get; }
	    string GameVersion { get; }
    }
}