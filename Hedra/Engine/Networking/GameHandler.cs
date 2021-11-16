namespace Hedra.Engine.Networking
{
    public abstract class GameHandler
    {
        protected GameHandler()
        {
            Connection = ConnectionFactory.Build(Type);
            Connection.MessageReceived += OnMessageReceived;
        }

        protected BaseConnection Connection { get; }
        protected abstract ConnectionType Type { get; }

        public abstract bool IsActive { get; protected set; }

        private void OnMessageReceived(ulong Sender, byte[] Message)
        {
            if (CommonMessages.IsCommonMessage(Message))
                HandleCommonMessage(Sender, CommonMessages.Parse(Message));
            else
                HandleNormalMessage(Sender, Message);
        }

        public void Update()
        {
            if (!IsActive) return;
            DoUpdate();
        }

        protected abstract void DoUpdate();

        protected abstract void HandleCommonMessage(ulong Sender, CommonMessageType Message);

        protected abstract void HandleNormalMessage(ulong Sender, byte[] Message);
    }
}