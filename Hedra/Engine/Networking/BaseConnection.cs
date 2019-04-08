using System;

namespace Hedra.Engine.Networking
{
    public delegate void OnMessageReceived(ulong Sender, byte[] Message);
    
    public abstract class BaseConnection : IDisposable
    {
        public ConnectionType Type { get; }

        protected BaseConnection(ConnectionType Type)
        {
            this.Type = Type;
        }
        
        public abstract event OnMessageReceived MessageReceived;
        
        public abstract void SendMessage(ulong Peer, byte[] Buffer, int Count);

        public void SendMessage(ulong Peer, byte[] Message)
        {
            SendMessage(Peer, Message, Message.Length);
        }

        public abstract void Setup();

        public virtual void Dispose()
        { 
        }
    }
}