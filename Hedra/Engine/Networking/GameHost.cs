using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.Networking.Packets;
using Hedra.Engine.Steamworks;

namespace Hedra.Engine.Networking
{
    public class GameHost : GameHandler
    {
        private const int MaxClients = 6;
        private const int MaxSecondsBeforeDisconnect = 30;
        private readonly Dictionary<ulong, PeerInformation> _clients;
        private readonly Timer _disconnectTimer;

        public GameHost()
        {
            _clients = new Dictionary<ulong, PeerInformation>();
            _disconnectTimer = new Timer(15);
        }
        
        public void Host()
        {
            if (IsActive) throw new ArgumentOutOfRangeException();
            Connection.Setup();
            IsActive = true;
        }

        public void Disconnect()
        {
            ForEach(I => Connection.SendMessage(I, CommonMessages.Kick));
            IsActive = false;
            _clients.Clear();
        }

        private void ForEach(Action<ulong> Action)
        {
            var asList = _clients.Keys.ToList();
            for (var i = 0; i < asList.Count; ++i)
            {
                Action(asList[i]);
            }
        }
        
        private void ForEachIf(Predicate<ulong> Condition, Action<ulong> Foreach)
        {
            var asList = _clients.Keys.ToList();
            for (var i = 0; i < asList.Count; ++i)
            {
                if(Condition(asList[i]))
                    Foreach(asList[i]);
            }
        }
        
        protected override void HandleCommonMessage(ulong Sender, CommonMessageType Message)
        {
            switch (Message)
            {
                case CommonMessageType.Join:
                    AnswerJoin(Sender);
                    break;
                case CommonMessageType.Disconnect:
                    DisconnectClient(Sender);
                    break;
                case CommonMessageType.Ping:
                    _clients[Sender].ProcessPing();
                    break;
            }
        }
        
        protected override void HandleNormalMessage(ulong Sender, byte[] Message)
        {
            if(Message.Length == 0) return;
            var header = (CommonMessageType) Message[0];
            if (header == CommonMessageType.Relay)
            {
                var id = BitConverter.ToUInt64(Message.Skip(1).Take(sizeof(ulong)).ToArray(), 0);
                var message = Message.Skip(sizeof(ulong) + 1).ToArray();
                Log.WriteLine($"Relaying normal message with length '{message.Length}' to user '{id}'");
                Connection.SendMessage(id, message);
            }
        }

        private void AnswerJoin(ulong Id)
        {
            if (IsFull)
            {
                Connection.SendMessage(Id, CommonMessages.DenyJoin);
            }
            else
            {
                Connection.SendMessage(Id, new AcceptJoinPacket
                {
                   Seed = World.Seed
                }.Serialize());
                AddPeer(Id);
            }
        }

        private void AddPeer(ulong Id)
        {
            _clients.Add(Id, new PeerInformation());
            UpdatePeers();
        }
        
        private void DisconnectClient(ulong Id)
        {
            _clients.Remove(Id);
            UpdatePeers();
        }

        private void UpdatePeers()
        {
            ForEach(U =>
            {
                Connection.SendMessage(U, new PeersPacket
                {
                    PeerIds = _clients.Keys.Where(P => P != U).ToArray()
                }.Serialize());
            });
        }
        
        protected override void DoUpdate()
        {
            if (_disconnectTimer.Tick())
            {
                ForEach(U =>
                {
                    if(_clients[U].SecondsSinceLastPing > MaxSecondsBeforeDisconnect)
                        DisconnectClient(U);
                });
            }
        }

        private bool IsFull => _clients.Count == MaxClients;
        public override bool IsActive { get; protected set; }
        protected override ConnectionType Type => ConnectionType.Host;
    }

    public class PeerInformation
    {
        private DateTime _lastPing;
        
        public void ProcessPing()
        {
            _lastPing = DateTime.Now;
        }
        
        public float SecondsSinceLastPing => (float) (DateTime.Now - _lastPing).TotalSeconds;
    }
}