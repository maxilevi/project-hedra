using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.IO;
using Hedra.Engine.Networking.Packets;
using Hedra.Engine.Steamworks;

namespace Hedra.Engine.Networking
{
    public class GameHost : GameHandler
    {
        private const int MaxClients = 4;
        private readonly HashSet<ulong> _clients;

        public GameHost()
        {
            _clients = new HashSet<ulong>();
        }
        
        public void Host()
        {
            if (IsActive) throw new ArgumentOutOfRangeException();
            Connection.Setup();
            IsActive = true;
        }

        public void Disconnect()
        {
            SendAll(I => Connection.SendMessage(I, CommonMessages.Kick));
            IsActive = false;
            _clients.Clear();
        }

        private void SendAll(Action<ulong> Foreach)
        {
            var asList = _clients.ToList();
            for (var i = 0; i < asList.Count; ++i)
            {
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
            }
        }
        
        protected override void HandleNormalMessage(ulong Sender, byte[] Message)
        {
            Log.WriteLine($"Received message with length '{Message.Length}'");
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
                _clients.Add(Id);
            }
        }

        private void DisconnectClient(ulong Id)
        {
            _clients.Remove(Id);
        }
        
        protected override void DoUpdate()
        {
            
        }

        private bool IsFull => _clients.Count == MaxClients;
        public override bool IsActive { get; protected set; }
        protected override ConnectionType Type => ConnectionType.Host;
    }
}