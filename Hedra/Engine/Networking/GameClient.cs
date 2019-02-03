namespace Hedra.Engine.Networking
{
    public class GameClient
    {
        public bool IsActive { get; private set; }

        public void Disconnect()
        {
            
        }
        
        public bool OnIncomingConnection(ulong SteamId)
        {
            return false;
        }

        public void OnP2PData(ulong SteamId, byte[] Data, int Length, int Channel)
        {
            
        }
    }
}