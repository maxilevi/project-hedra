using System;
using System.IO;
using Facepunch.Steamworks;
using Hedra.Engine.IO;
using Hedra.Framework;

namespace Hedra.Engine.Steamworks
{
    public class Steam : Singleton<Steam>
    {
        private const int GameId = 1009960;
        private bool _useSteam;

        public static FriendsWrapper Friends => FriendsWrapper.Instance;

        public static NetworkingWrapper Networking => NetworkingWrapper.Instance;

        public static LobbyWrapper Lobby => LobbyWrapper.Instance;

        public Client Client { get; private set; }

        public bool IsAvailable => _useSteam && Client.IsValid;

        private void SetupIfExists()
        {
            _useSteam = true;
            try
            {
                Client = new Client(GameId);
            }
            catch (Exception e) when (e is DllNotFoundException || e is FileNotFoundException)
            {
                Log.WriteLine($"Failed to load Steam library: {e}");
                _useSteam = false;
            }

            FriendsWrapper.Instance.SetSource(Client?.IsValid ?? false ? Client.Friends : null);
            NetworkingWrapper.Instance.SetSource(Client?.IsValid ?? false ? Client.Networking : null);
            LobbyWrapper.Instance.SetSource(Client?.IsValid ?? false ? Client.Lobby : null);
        }

        public void Initialize()
        {
            if (Client?.IsValid ?? false)
                AchievementsObserver.Initialize();
        }

        public static void Update()
        {
            Instance.Client?.Update();
        }

        public void Load()
        {
            SetupIfExists();
        }

        public void CallIf(Action<Client> Do)
        {
            if (IsAvailable)
                Do(Client);
        }

        public void Dispose()
        {
            Client.Dispose();
        }
    }
}