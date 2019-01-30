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
        private Client _client;
        private List<ulong> _peers;
        private bool _isHosting;
        
        public void Build()
        {
            _peers = new List<ulong>();
            if (Steam.Instance.IsAvailable)
                _client = Steam.Instance.GetClient();
            else
                return;
        }

        public void Host()
        {
            if (_isHosting) throw new ArgumentOutOfRangeException();
            _client.Lobby.OnLobbyCreated += delegate(bool B)
            {
                _client.Lobby.Name = "Project Hedra Lobby";
                _client.Lobby.Joinable = true;
                _isHosting = B;
            };
            _client.Lobby.OnLobbyMemberDataUpdated += delegate(ulong U)
            {
                _peers.Add(U);
            };
            _client.Lobby.Create(Lobby.Type.FriendsOnly, 8);
        }

        public void Disconnect()
        {
            _client.Lobby.Leave();
        }

        public bool IsAlive => _client != null;
        
        public void Update()
        {
            if(_client != null)
                _client.Update();   
        }
    }
}