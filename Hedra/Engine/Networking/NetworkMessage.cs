using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.IO;
using Hedra.Engine.Networking.Packets;

namespace Hedra.Engine.Networking
{
    public class NetworkMessage
    {
        private static readonly List<INetworkPacket> PacketTypes = new List<INetworkPacket>();

        static NetworkMessage()
        {
            var typeList = Assembly.GetExecutingAssembly().GetLoadableTypes(typeof(NetworkMessage).Namespace).ToArray();
            for(var i = 0; i < typeList.Length; ++i)
            {
                if(!typeList[i].IsAssignableFrom(typeof(INetworkPacket)) || typeList[i].IsAbstract) continue;

                var packet = (INetworkPacket) Activator.CreateInstance(typeList[i]);
                PacketTypes.Add(packet);
            }
        }
        
        public static void ParseIfType<T>(byte[] Message, Action<T> Lambda) where T : INetworkPacket, new()
        {
            if(Message.Length == 0)
                throw new ArgumentOutOfRangeException($"Network packet is empty");
            
            var type = (MessagePacketType) Message[0];
            var expectedType = typeof(T);
            for (var i = 0; i < PacketTypes.Count; ++i)
            {
                if (PacketTypes[i].GetType() == expectedType && type == PacketTypes[i].Type)
                {
                    var packet = (NetworkPacket<T>) PacketTypes[i];
                    Lambda(packet.Parse(Message.Skip(1).ToArray()));
                }
            }
        }
    }

    public enum MessagePacketType
    {
        WorldPacket,
        AcceptJointPacket,
        PlayerInformationPacket
    }
}