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
        public override ulong Myself => 1;
        public override event OnMessageReceived MessageReceived;
        private bool _created;
        private bool _isListening;
        private Thread _thread;
        private ulong _lastId;
        private readonly Dictionary<ulong, TcpClient> _peers;
        private TcpClient _client;
        private TcpListener _listener;
        
        public LocalConnection(ConnectionType Type) : base(Type)
        {
            _lastId = Myself;
            _peers = new Dictionary<ulong, TcpClient>();
        }

        public override void Setup()
        {
            if(Type == ConnectionType.Client)
                SetupClient();
            else if(Type == ConnectionType.Host)
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
                    var id = _lastId++; 
                    BuildClientLoop(id, client);
                }
            }
            CreateLoop(ServerLoop);
        }

        private void SetupClient()
        {
            _created = true;
            _client = new TcpClient("127.0.0.1", 5000);
            _isListening = true;
            BuildClientLoop(Myself, _client);
        }


        private void CreateLoop(ThreadStart Loop)
        {
            _thread = new Thread(Loop);
            _thread.Start();
        }

        private void BuildClientLoop(ulong Id, TcpClient Client)
        {
            _peers.Add(Id, Client);
            void ClientLoop()
            {
                var buffer = new byte[4096];
                while (Program.GameWindow.Exists)
                {
                    var count = Client.Client.Receive(buffer);
                    if (count == 0) break;
                    ReceiveMessage(Id, buffer.Take(count).ToArray());
                }
                _peers.Remove(Id);
                Log.WriteLine("A client has disconnected from the server");
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
            var stream = _peers[Peer].GetStream();
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