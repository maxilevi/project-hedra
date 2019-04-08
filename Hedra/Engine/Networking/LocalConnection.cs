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
        private const int DummyId = 1;
        public override event OnMessageReceived MessageReceived;
        private readonly List<Action> _writeOrders;
        private bool _created;
        private bool _isListening;
        private Thread _thread;
        private TcpClient _client;
        private TcpListener _listener;
        
        public LocalConnection(ConnectionType Type) : base(Type)
        {
            _writeOrders = new List<Action>();
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
            void Loop()
            {
                Log.WriteLine($"Started local connection loop of type '{Type}'...");
                _listener.Start();
                _client = _listener.AcceptTcpClient();
                _isListening = true;
                var buffer = new byte[4096];
                while (Program.GameWindow.Exists)
                {
                    var count = _client.Client.Receive(buffer);
                    if (count == 0) continue;
                    ReceiveMessage(buffer.Take(count).ToArray());
                }
            }
            CreateLoop(Loop);
        }

        private void SetupClient()
        {
            _created = true;
            _client = new TcpClient("127.0.0.1", 5000);
            _isListening = true;
            void Loop()
            {
                var buffer = new byte[4096];
                while (Program.GameWindow.Exists)
                {
                    while (_writeOrders.Count == 0){}
                    _writeOrders[0]();
                    _writeOrders.RemoveAt(0);
                    var count = _client.Client.Receive(buffer);
                    ReceiveMessage(buffer.Take(count).ToArray());
                }
            }
            CreateLoop(Loop);
        }


        private void CreateLoop(ThreadStart Loop)
        {
            _thread = new Thread(Loop);
            _thread.Start();
        }

        private void ReceiveMessage(byte[] Message)
        {
            Log.WriteLine($"Received message of length '{Message.Length}'");
            MessageReceived?.Invoke(DummyId, Message);
        }

        public override void SendMessage(ulong Peer, byte[] Buffer, int Count)
        {
            void WriteAction()
            {
                var stream = _client.GetStream();
                stream.Write(Buffer, 0, Count);
                stream.Flush();
            }
            _writeOrders.Add(WriteAction);
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