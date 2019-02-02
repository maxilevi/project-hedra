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
        
        public void Load()
        {
            SetupIfExists();
        }

        public void CallIf(Action<Client> Do)
        {
            if(IsAvailable)
                Do(_client);
        }

        public bool IsAvailable => _useSteam && _client.IsValid;
        
        public Client GetClient()
        {
            if(!IsAvailable) throw new ArgumentOutOfRangeException($"Cannot use networking features without steam.");
            return _client;
        }
        
        public void Dispose()
        {
            _client.Dispose();
        }
    }
}