using System;
using System.Collections.Generic;
using Facepunch.Steamworks;
using Hedra.Engine.Core;
using Hedra.Engine.IO;
using Hedra.Engine.Steamworks;

namespace Hedra.Engine.Networking
{
    public class Connection : Singleton<Connection>
    {
        private GameHost _host;
        private GameClient _client;
        public bool IsAlive => Steam.Instance.IsAvailable && (_host.IsActive || _client.IsActive);

        public void Host()
        {
            if (!Steam.Instance.IsAvailable) return;
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
        
        public static void Load()
        {
            Instance.Build();
        }

        private void Build()
        {
            if (!Steam.Instance.IsAvailable) return;
            _host = new GameHost();
            _client = new GameClient();
            Steam.Instance.CallIf(C =>
            {
                C.Networking.OnIncomingConnection += _host.OnIncomingConnection;
                C.Networking.OnIncomingConnection += _client.OnIncomingConnection;
                C.Networking.OnP2PData += _host.OnP2PData;
                C.Networking.OnP2PData += _client.OnP2PData;
            });
        }

        public void InviteFriends()
        {
            Steam.Instance.CallIf(C => C.Overlay.OpenUserPage("friends", C.OwnerSteamId));
        }
    }
}