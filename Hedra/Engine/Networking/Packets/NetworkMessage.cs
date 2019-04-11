using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hedra.Engine.IO;

namespace Hedra.Engine.Networking.Packets
{
    public class NetworkMessage
    {
        private static bool _loaded;
        private static readonly List<INetworkPacket<IPacket>> PacketTypes = new List<INetworkPacket<IPacket>>();

        static NetworkMessage()
        {
            Load();
        }
        
        public static void Load()
        {
            if(_loaded) return;
            var typeList = Assembly.GetExecutingAssembly().GetLoadableTypes(typeof(NetworkMessage).Namespace).ToArray();
            Log.WriteLine($"Network types found ('{typeList.Length}')");
            for(var i = 0; i < typeList.Length; ++i)
            {
                var isNetworkPacket = typeof(INetworkPacket<IPacket>).IsAssignableFrom(typeList[i]);
                if (!isNetworkPacket || typeList[i].IsAbstract) continue;

                var packet = (INetworkPacket<IPacket>) Activator.CreateInstance(typeList[i]);
                PacketTypes.Add(packet);
            }
            Log.WriteLine($"Discovered '{PacketTypes.Count}' different hedra packets...");
            _loaded = true;
        }
        
        public static void ParseIfType<T>(byte[] Message, Action<T> Lambda) where T : INetworkPacket<T>
        {
            if(Message.Length == 0)
                throw new ArgumentOutOfRangeException($"Network packet is empty");
            
            var type = (MessagePacketType) Message[0];
            for (var i = 0; i < PacketTypes.Count; ++i)
            {
                if (type == PacketTypes[i].Type && PacketTypes[i] is T)
                {
                    var packet = (T) PacketTypes[i];
                    Lambda(packet.Parse(Message.Skip(1).ToArray()));
                }
            }
        }
    }

    public enum MessagePacketType : byte
    {
        WorldPacket,
        AcceptJointPacket,
        PlayerInformationPacket,
        PeersPacket,
        AnimationStatePacket,
        PlayerAttributesPacket,
        PositionAndRotationPacket,
    }
}