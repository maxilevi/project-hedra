namespace Hedra.Engine.Networking
{
    public abstract class GameHandler
    {
        protected BaseConnection Connection { get; }
        protected abstract ConnectionType Type { get; }

        protected GameHandler()
        {
            Connection = ConnectionFactory.Build(Type);
            Connection.MessageReceived += OnMessageReceived;
        }
        
        private void OnMessageReceived(ulong Sender, byte[] Message)
        {
            if (CommonMessages.IsCommonMessage(Message))
                HandleCommonMessage(Sender, CommonMessages.Parse(Message));
            else
                HandleNormalMessage(Sender, Message);
        }

        public void Update()
        {
            if(!IsActive) return;
            DoUpdate();
        }

        protected abstract void DoUpdate();

        protected abstract void HandleCommonMessage(ulong Sender, CommonMessageType Message);
        
        protected abstract void HandleNormalMessage(ulong Sender, byte[] Message);
        
        public abstract bool IsActive { get; protected set; }
    }
}