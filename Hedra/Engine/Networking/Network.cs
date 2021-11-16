using Hedra.Engine.Networking.Packets;
using Hedra.Engine.Steamworks;
using Hedra.Framework;

namespace Hedra.Engine.Networking
{
    public class Network : Singleton<Network>
    {
        private readonly GameClient _client;
        private readonly GameHost _host;

        public Network()
        {
            _host = new GameHost();
            _client = new GameClient();
            NetworkMessage.Load();
        }

        public bool IsAlive => _host.IsActive || _client.IsActive;

        public void Host()
        {
            Disconnect();
            _host.Host();
            _client.ConnectLocally();
        }

        public void Disconnect()
        {
            if (!Steam.Instance.IsAvailable) return;
            if (_host.IsActive)
                _host.Disconnect();
            _client.Disconnect();
        }

        public void Update()
        {
            _host.Update();
            _client.Update();
        }

        public void Connect(ulong Id)
        {
            Disconnect();
            _client.RequestJoin(Id);
        }

        public void InviteFriends()
        {
            Steam.Instance.CallIf(C => C.Overlay.OpenUserPage("friends", C.OwnerSteamId));
        }
    }
}