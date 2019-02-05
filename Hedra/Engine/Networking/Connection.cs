using System;
using System.Collections.Generic;
using Hedra.Engine.Core;


namespace Hedra.Engine.Networking
{
    public class Connection : Singleton<Connection>
    {
        private object _client;
        private List<ulong> _peers;
        private bool _isHosting;
        
        public void Build()
        {

        }

        public void Host()
        {

        }

        public bool IsAlive => _client != null;
        
        public void Update()
        {
 
        }
    }
}