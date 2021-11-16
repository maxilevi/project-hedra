using System;

namespace Hedra.Engine.Networking
{
    public delegate void OnMessageReceived(ulong Sender, byte[] Message);

    public abstract class BaseConnection : IDisposable
    {
        protected BaseConnection(ConnectionType Type)
        {
            this.Type = Type;
        }

        public ConnectionType Type { get; }

        public abstract ulong Myself { get; }

        public virtual void Dispose()
        {
        }

        public abstract event OnMessageReceived MessageReceived;

        public abstract void SendMessage(ulong Peer, byte[] Buffer, int Count);

        public void SendMessage(ulong Peer, byte[] Message)
        {
            SendMessage(Peer, Message, Message.Length);
        }

        public abstract void Setup();
    }
}