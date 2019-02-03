using System;
using System.Collections.Generic;
using Hedra.Engine.Steamworks;

namespace Hedra.Engine.Networking
{
    public class GameHost
    {
        private static readonly byte[] KickedMessage = new byte[0];
        private readonly List<ulong> _peers;
        private bool _isHosting;

        public GameHost()
        {
            _peers = new List<ulong>();
        }
        
        public void Host()
        {
            if (_isHosting) throw new ArgumentOutOfRangeException();
            _isHosting = true;
        }

        public void Disconnect()
        {
            for (var i = 0; i < _peers.Count; ++i)
            {
                Steam.Networking.SendP2PPacket(_peers[i], KickedMessage, KickedMessage.Length);
            }
            _isHosting = false;
            _peers.Clear();
        }
        
                
        public bool OnIncomingConnection(ulong SteamId)
        {
            if (Steam.Friends.Get(SteamId) == null) return false;
            return CanAcceptConnections;
        }

        public void OnP2PData(ulong SteamId, byte[] Data, int Length, int Channel)
        {
            
        }

        public bool IsActive => _isHosting;
        private bool CanAcceptConnections => _isHosting;
    }
}