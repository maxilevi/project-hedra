namespace Hedra.Engine.Networking
{
    public static class CommonMessages
    {
        public static readonly byte[] Join = { (byte)CommonMessageType.Join };
        public static readonly byte[] Kick = { (byte)CommonMessageType.Kick };
        public static readonly byte[] Disconnect = { (byte)CommonMessageType.Disconnect };
        public static readonly byte[] DenyJoin = { (byte)CommonMessageType.DenyJoin };
        public static readonly byte[] Ping = { (byte)CommonMessageType.Ping };
        public static readonly byte[] AskPlayerInformation = { (byte)CommonMessageType.AskPlayerInformation };
        public static readonly byte[] Relay = { (byte)CommonMessageType.Relay };

        public static bool IsCommonMessage(byte[] Message)
        {
            if (Message.Length != 1) return false;
            return Message[0] < (byte)CommonMessageType.MaxType;
        }

        public static CommonMessageType Parse(byte[] Message)
        {
            return (CommonMessageType)Message[0];
        }
    }

    public enum CommonMessageType : byte
    {
        Join = 0,
        Kick,
        Disconnect,
        Ping,
        DenyJoin,
        AskPlayerInformation,
        Relay,
        MaxType
    }
}