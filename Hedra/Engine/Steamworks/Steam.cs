using System;
using System.IO;
using Facepunch.Steamworks;
using Hedra.Engine.Core;
using Hedra.Engine.IO;

namespace Hedra.Engine.Steamworks
{
    public class Steam : Singleton<Steam>
    {
        private Client _client;
        private bool _useSteam;
        
        private void SetupIfExists()
        {
            _useSteam = true;
            try
            {
                _client = new Client(1009960);
            }
            catch(Exception e) when (e is DllNotFoundException || e is FileNotFoundException)
            {
                Log.WriteLine($"Failed to load Steam library: {e}");
                _useSteam = false;
            }
        }

        public static void Update()
        {
            Instance._client?.Update();
        }
        
        public void Load()
        {
            SetupIfExists();
        }

        public void CallIf(Action<Client> Do)
        {
            if(IsAvailable)
                Do(_client);
        }

        public static Friends Friends => Instance._client.Friends;
        
        public static Facepunch.Steamworks.Networking Networking => Instance._client.Networking;
        
        public static Lobby Lobby => Instance._client.Lobby;
        
        public bool IsAvailable => _useSteam && _client.IsValid;
        
        public void Dispose()
        {
            _client.Dispose();
        }
    }
}