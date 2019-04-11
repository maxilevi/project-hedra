using Hedra.Engine.Networking.Packets;
using NUnit.Framework;

namespace HedraTests.Network
{
    [TestFixture]
    public class NetworkMessageTest
    {
        [Test]
        public void TestPacketParsing()
        {
            var packet = new AcceptJoinPacket
            {
                Seed = 1
            }.Serialize();
            var wasCalled = false;
            NetworkMessage.ParseIfType<AcceptJoinPacket>(packet, Packet =>
            {
                wasCalled = true;
                Assert.NotNull(Packet);
                Assert.AreEqual(1, Packet.Seed);
            });
            Assert.True(wasCalled);
        }
        
        [Test]
        public void TestPacketMatching()
        {
            var packet = new AcceptJoinPacket
            {
                Seed = 1
            }
            .Serialize();

            NetworkMessage.ParseIfType<PeersPacket>(packet, Packet =>
            {
                Assert.Fail("Matched incorrect packet type.");
            });
        }

        [Test]
        public void TestPeersSerialization()
        {
            var packet = new PeersPacket
            {
                PeerIds = new ulong[]
                {
                    1000
                }
            }
            .Serialize();

            var wasCalled = false;
            NetworkMessage.ParseIfType<PeersPacket>(packet, Packet =>
            {
                wasCalled = true;
                Assert.NotNull(packet);
                Assert.AreEqual(new ulong[]
                {
                    1000
                }, Packet.PeerIds);
            });
            
            Assert.True(wasCalled);
        }
    }
}