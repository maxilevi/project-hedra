namespace Hedra.Engine.Steamworks
{
    public class NetworkingWrapper : SteamObjectWrapper<NetworkingWrapper, Facepunch.Steamworks.Networking>
    {
        public bool SendP2PPacket(ulong SteamId, byte[] Data, int Length)
        {
            return Source.SendP2PPacket(SteamId, Data, Length);
        }
    }
}