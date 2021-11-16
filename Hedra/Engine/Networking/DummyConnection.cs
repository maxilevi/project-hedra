using System;

namespace Hedra.Engine.Networking
{
    public class DummyConnection : BaseConnection
    {
        public DummyConnection(ConnectionType Type) : base(Type)
        {
        }

        public override ulong Myself { get; }
        public override event OnMessageReceived MessageReceived;

        public override void SendMessage(ulong Peer, byte[] Buffer, int Count)
        {
            throw new NotImplementedException();
        }

        public override void Setup()
        {
            throw new NotImplementedException();
        }
    }
}