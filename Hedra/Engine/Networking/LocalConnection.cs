using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Hedra.Engine.IO;

namespace Hedra.Engine.Networking
{
    public class LocalConnection : BaseConnection, IConnection
    {
        private readonly Dictionary<ulong, TcpClient> _peers;
        private TcpClient _client;
        private bool _created;
        private bool _isListening;
        private TcpListener _listener;
        private ulong _serverId;
        private Thread _thread;

        public LocalConnection(ConnectionType Type) : base(Type)
        {
            Myself = (ulong)Environment.TickCount;
            _peers = new Dictionary<ulong, TcpClient>();
        }

        public override ulong Myself { get; }
        public override event OnMessageReceived MessageReceived;

        public override void Setup()
        {
            if (Type == ConnectionType.Client)
                SetupClient();
            else if (Type == ConnectionType.Host)
                SetupHost();
        }

        private void SetupHost()
        {
            _created = true;
            _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 5000);

            void ServerLoop()
            {
                Log.WriteLine($"Started local connection loop of type '{Type}'...");
                _listener.Start();
                _isListening = true;
                while (Program.GameWindow.Exists)
                {
                    var client = _listener.AcceptTcpClient();
                    SendMessage(client, BitConverter.GetBytes(Myself), sizeof(ulong));
                    BuildClientLoop(client);
                }
            }

            CreateLoop(ServerLoop);
        }

        private void SetupClient()
        {
            _created = true;
            _client = new TcpClient("127.0.0.1", 5000);
            _isListening = true;
            SendMessage(_client, BitConverter.GetBytes(Myself), sizeof(ulong));
            BuildClientLoop(_client);
        }


        private void CreateLoop(ThreadStart Loop)
        {
            _thread = new Thread(Loop);
            _thread.Start();
        }

        private void BuildClientLoop(TcpClient Client)
        {
            void ClientLoop()
            {
                var buffer = new byte[4096];
                var registered = false;
                var id = 0ul;
                while (Program.GameWindow.Exists)
                {
                    var count = Client.Client.Receive(buffer, 0, buffer.Length, SocketFlags.None, out var error);
                    if (error != SocketError.Success) break;
                    if (registered)
                    {
                        ReceiveMessage(id, buffer.Take(count).ToArray());
                    }
                    else
                    {
                        id = BitConverter.ToUInt64(buffer.Take(count).ToArray(), 0);
                        _peers.Add(id, Client);
                        registered = true;
                        if (ConnectionType.Client == Type)
                            _serverId = id;
                    }
                }

                _peers.Remove(id);
                Log.WriteLine($"A client '{id}' has disconnected from the server");
            }

            CreateLoop(ClientLoop);
        }


        private void ReceiveMessage(ulong Sender, byte[] Message)
        {
            Log.WriteLine($"Received message of length '{Message.Length}' from '{Sender}'");
            MessageReceived?.Invoke(Sender, Message);
        }

        public override void SendMessage(ulong Peer, byte[] Buffer, int Count)
        {
            if (!_peers.ContainsKey(Peer))
            {
                var target = Peer;
                Peer = _serverId;
                Buffer = CommonMessages.Relay.Concat(BitConverter.GetBytes(target)).Concat(Buffer).ToArray();
                Count = Buffer.Length;
            }

            SendMessage(_peers[Peer], Buffer, Count);
        }

        private void SendMessage(TcpClient Client, byte[] Buffer, int Count)
        {
            var stream = Client.GetStream();
            stream.Write(Buffer, 0, Count);
            stream.Flush();
        }

        public override void Dispose()
        {
        }
    }

    public enum ConnectionType
    {
        Host,
        Client
    }
}