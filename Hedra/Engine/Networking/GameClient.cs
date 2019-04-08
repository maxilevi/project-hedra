using System;
using Hedra.Engine.Game;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.Networking.Packets;

namespace Hedra.Engine.Networking
{
    public class GameClient : GameHandler
    {
        protected override ConnectionType Type => ConnectionType.Client;
        private ulong _currentServer;
        private readonly Timer _pingTimer = new Timer(1);
        
        public void RequestJoin(ulong Id)
        {
            Log.WriteLine($"Requesting join to user '{Id}'...");
            Connection.Setup();
            if(IsActive) Disconnect();
            Log.WriteLine($"Sending join message to '{Id}'...");
            Connection.SendMessage(Id, CommonMessages.Join);
        }

        protected override void DoUpdate()
        {
            //if(_pingTimer.Tick()) Connection.SendMessage(_currentServer, CommonMessages.Ping);
        }

        protected override void HandleNormalMessage(ulong Sender, byte[] Message)
        {
            NetworkMessage.ParseIfType<AcceptJoinPacket>(Message, A => DoConnect(Sender, A));
        }

        protected override void HandleCommonMessage(ulong Sender, CommonMessageType Message)
        {
            switch (Message)
            {
                case CommonMessageType.Kick:
                    Disconnect();
                    break;
            }
        }

        private void DoConnect(ulong Sender, WorldPacket Packet)
        {
            _currentServer = Sender;
            IsActive = true;
            World.Recreate(Packet.Seed);
        }
        
        public void Disconnect()
        {
            if(!IsActive)
                throw new ArgumentException("Client is not connected to a server.");
            Connection.SendMessage(_currentServer, CommonMessages.Disconnect);
            _currentServer = 0;
            IsActive = false;
        }
        
        public override bool IsActive { get; protected set; }
    }
}