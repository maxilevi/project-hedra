using Hedra.Engine.Game;
using Hedra.Engine.Steamworks;
using Hedra.Game;

namespace Hedra.Engine.Networking
{
    public static class ConnectionFactory
    {
        public static BaseConnection Build(ConnectionType Type)
        {
            if(!Steam.Instance.IsAvailable && GameSettings.DebugMode)
                return new LocalConnection(Type);
            
            else if (Steam.Instance.IsAvailable)
                return new SteamConnection(Type);
            
            return new DummyConnection(Type);
                
        }
    }
}