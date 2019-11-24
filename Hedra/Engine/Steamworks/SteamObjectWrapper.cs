using System.IO;
using System.Net;
using Hedra.Engine.Core;
using Hedra.Engine.Management;
using Hedra.Framework;

namespace Hedra.Engine.Steamworks
{
    public class SteamObjectWrapper<T, U> : Singleton<T> where T : class, new()
    {
        protected U Source { get; private set; }
        
        public void SetSource(U Source)
        {
            this.Source = Source;
        }
    }
}