using System;
using Hedra.Core;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.Networking.Packets;
using Hedra.Engine.Player;

namespace Hedra.Engine.Networking
{
    public class GameClient : GameHandler
    {
        private const float UpdateRate = 0.33f;
        private readonly Timer _pingTimer;
        private readonly Timer _positionTimer;
        private readonly NetworkWorld _world;
        private ulong _currentServer;

        public GameClient()
        {
            _world = new NetworkWorld();
            _pingTimer = new Timer(4);
            _positionTimer = new Timer(UpdateRate);
        }

        protected override ConnectionType Type => ConnectionType.Client;

        public override bool IsActive { get; protected set; }

        public void RequestJoin(ulong Id)
        {
            Log.WriteLine($"Requesting join to user '{Id}'...");
            Connection.Setup();
            if (IsActive) Disconnect();
            Log.WriteLine($"Sending join message to '{Id}'...");
            Connection.SendMessage(Id, CommonMessages.Join);
        }

        public void ConnectLocally()
        {
            RequestJoin(Connection.Myself);
        }

        protected override void DoUpdate()
        {
            if (_pingTimer.Tick()) Connection.SendMessage(_currentServer, CommonMessages.Ping);
            if (_positionTimer.Tick()) _world.Do(SendPlayerPositionPacket);
        }

        protected override void HandleNormalMessage(ulong Sender, byte[] Message)
        {
            Log.WriteLine(
                $"Received normal message of length '{Message.Length}' with header {(MessagePacketType)Message[0]}");

            NetworkMessage.ParseIfType<AcceptJoinPacket>(Message, A => DoConnect(Sender, A));
            NetworkMessage.ParseIfType<PeersPacket>(Message, A => ReceivePeers(Sender, A));
            NetworkMessage.ParseIfType<PlayerInformationPacket>(Message, A => UpdateInformation(Sender, A));
            NetworkMessage.ParseIfType<PositionAndRotationPacket>(Message, A => UpdatePositionAndRotation(Sender, A));
        }

        protected override void HandleCommonMessage(ulong Sender, CommonMessageType Message)
        {
            Log.WriteLine($"Received common message of type '{Message}' from '{Sender}'");
            switch (Message)
            {
                case CommonMessageType.Kick:
                    Disconnect();
                    break;
                case CommonMessageType.AskPlayerInformation:
                    SendPlayerInformation(Sender, _currentServer);
                    break;
            }
        }

        private void DoConnect(ulong Sender, AcceptJoinPacket Packet)
        {
            IsActive = true;
            _currentServer = Sender;
            Log.WriteLine($"Connected to server '{Sender}' with seed '{Packet.Seed}'");
            Executer.ExecuteOnMainThread(() => World.Recreate(Packet.Seed));
        }

        private void ReceivePeers(ulong Sender, PeersPacket Packet)
        {
            Log.WriteLine($"Received peer list of length '{Packet.PeerIds.Length}'");
            var delta = _world.UpdateAndGetDelta(Packet.PeerIds);
            for (var i = 0; i < delta.Length; ++i)
                Connection.SendMessage(delta[i], CommonMessages.AskPlayerInformation);
        }

        private void SendPlayerPositionPacket(ulong Id)
        {
            Connection.SendMessage(Id,
                PositionAndRotationPacket.From(LocalPlayer.Instance, Connection.Myself).Serialize());
        }

        private void SendPlayerInformation(ulong Sender, ulong Myself)
        {
            Connection.SendMessage(Sender, PlayerInformationPacket.From(Myself, LocalPlayer.Instance).Serialize());
        }

        private void UpdateInformation(ulong Sender, PlayerInformationPacket Packet)
        {
            _world.UpdatePeerInformation(Packet);
        }

        private void UpdatePositionAndRotation(ulong Sender, PositionAndRotationPacket Packet)
        {
            _world.UpdatePositionAndRotation(Packet);
        }

        public void Disconnect()
        {
            if (!IsActive)
                throw new ArgumentException("Client is not connected to a server.");
            Connection.SendMessage(_currentServer, CommonMessages.Disconnect);
            _currentServer = 0;
            IsActive = false;
        }
    }
}