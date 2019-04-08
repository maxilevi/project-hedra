using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Facepunch.Steamworks;
using Hedra.Engine.Core;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.Steamworks;

namespace Hedra.Engine.Networking
{
    public class Network : Singleton<Network>
    {
        private readonly GameHost _host = new GameHost();
        private readonly GameClient _client = new GameClient();
        public bool IsAlive => Steam.Instance.IsAvailable && (_host.IsActive || _client.IsActive);

        public void Host()
        {
            //if (!Steam.Instance.IsAvailable) return;
            _host.Host();
        }
        
        public void Disconnect()
        {
            if (!Steam.Instance.IsAvailable) return;
            if (_host.IsActive)
                _host.Disconnect(); 
            else
                _client.Disconnect();
        }

        public void Update()
        {
            _host.Update();
            _client.Update();
        }
        
        public void Connect(ulong Id)
        {
            _client.RequestJoin(Id);
        }

        public void InviteFriends()
        {
            Steam.Instance.CallIf(C => C.Overlay.OpenUserPage("friends", C.OwnerSteamId));
        }
    }
}