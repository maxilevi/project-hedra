using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Core;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class LoadBalancer
    {
        private readonly HashSet<ICountable> _users;
        private readonly Dictionary<ICountable, float> _loads;
        public int Capacity { get; }

        public LoadBalancer(int Capacity)
        {
            this.Capacity = Capacity;
            this._users = new HashSet<ICountable>();
            this._loads = new Dictionary<ICountable, float>();
        }

        public void Register(ICountable User)
        {
            _users.Add(User);
            _loads.Add(User, 0);
        }

        public void Unregister(ICountable User)
        {
            _users.Remove(User);
            _loads.Remove(User);
        }

        private int GetLoad(ICountable Index)
        {
            return (int) Math.Ceiling(_loads[Index] * Capacity);
        }

        public bool Contains(ICountable Index)
        {
            return _users.Contains(Index);
        }

        public int this[ICountable Index] => (int)Capacity;
    }
}