using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Components;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.Networking.Packets;
using Hedra.Engine.Player;
using Hedra.Engine.Player.Networking;
using Hedra.EntitySystem;
using System.Numerics;

namespace Hedra.Engine.Networking
{
    public class NetworkPeer : IDisposable
    {
        public IHumanoid Humanoid { get; }

        public NetworkPeer(IHumanoid Humanoid)
        {
            this.Humanoid = Humanoid;
        }

        public void Dispose()
        {
            Humanoid.Dispose();
        }
    }

    public class NetworkWorld
    {
        private readonly Dictionary<ulong, NetworkPeer> _peers;

        public NetworkWorld()
        {
            _peers = new Dictionary<ulong, NetworkPeer>();
        }

        public ulong[] UpdateAndGetDelta(ulong[] Peers)
        {
            var deletedPeers = _peers.Keys.Where(P => Array.IndexOf(Peers, P) == -1).ToArray();
            DisposePeers(deletedPeers);
            var delta = Peers.Where(P => !_peers.ContainsKey(P)).ToArray();
            return delta;
        }

        public void Do(Action<ulong> Do)
        {
            DoIf(U => true, Do);
        }
        
        public void DoIf(Predicate<ulong> If, Action<ulong> Do)
        {
            var keys = _peers.Keys.ToList();
            for (var i = 0; i < keys.Count; ++i)
            {
                if (If(keys[i]))
                    Do(keys[i]);
            }
        }
        
        private void DisposePeers(params ulong[] Peers)
        {
            for (var i = 0; i < Peers.Length; ++i)
            {
                _peers[Peers[i]].Dispose();
            }
        }

        public void UpdatePeerInformation(PlayerInformationPacket Packet)
        {
            if(!_peers.ContainsKey(Packet.Id))
                _peers.Add(Packet.Id, new NetworkPeer(BuildHumanoid(Packet.Position)));
            Executer.ExecuteOnMainThread(() =>
            {
                PlayerInformationPacket.Apply(_peers[Packet.Id].Humanoid, Packet);
            });
        }

        public void UpdatePositionAndRotation(PositionAndRotationPacket Packet)
        {
            if (_peers.ContainsKey(Packet.Id))
            {
                _peers[Packet.Id].Humanoid.Position = Packet.Position;
                _peers[Packet.Id].Humanoid.Rotation = Packet.Rotation;
            }
            else
            {
                /* Check for mobs too */
            }
        }

        private static IHumanoid BuildHumanoid(Vector3 Position)
        {
            var human = new Humanoid();
            human.Position = Position;
            human.Physics.CollidesWithStructures = false;
            human.Physics.CollidesWithEntities = true;
            human.Model = new NetworkHumanoidModel(human);
            human.SearchComponent<DamageComponent>().Immune = true;
            return human;
        }
        
        public NetworkPeer this[ulong Id] => _peers[Id];
    }
}