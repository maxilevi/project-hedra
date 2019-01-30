using System;
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
            catch(DllNotFoundException e)
            {
                Log.WriteLine($"Failed to load Steam library: {e}");
                _useSteam = false;
            }
        }
        
        public void Load()
        {
            SetupIfExists();
            Console.WriteLine(_client.Username);
        }

        public void CallIf(Action<Client> Do)
        {
            if(_useSteam)
                Do(_client);
        }

        public bool IsAvailable => _useSteam;
        
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