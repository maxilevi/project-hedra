using System;
using System.IO;
using Facepunch.Steamworks;
using Hedra.Engine.Core;
using Hedra.Engine.IO;
using Hedra.Framework;

namespace Hedra.Engine.Steamworks
{
    public class Steam : Singleton<Steam>
    {
        private const int GameId = 1009960;
        private Client _client;
        private bool _useSteam;
        
        private void SetupIfExists()
        {
            _useSteam = true;
            try
            {
                _client = new Client(GameId);
            }
            catch(Exception e) when (e is DllNotFoundException || e is FileNotFoundException)
            {
                Log.WriteLine($"Failed to load Steam library: {e}");
                _useSteam = false;
            }
            FriendsWrapper.Instance.SetSource((_client?.IsValid ?? false) ? _client.Friends : null);
            NetworkingWrapper.Instance.SetSource((_client?.IsValid ?? false) ? _client.Networking : null);
            LobbyWrapper.Instance.SetSource((_client?.IsValid ?? false) ? _client.Lobby : null);
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

        public static FriendsWrapper Friends => FriendsWrapper.Instance;
        
        public static NetworkingWrapper Networking => NetworkingWrapper.Instance;
        
        public static LobbyWrapper Lobby => LobbyWrapper.Instance;
        
        public bool IsAvailable => _useSteam && _client.IsValid;
        
        public void Dispose()
        {
            _client.Dispose();
        }
    }
}