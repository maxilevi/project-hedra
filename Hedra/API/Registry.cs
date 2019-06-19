using System;
using System.Collections.Generic;

namespace Hedra.API
{
    public abstract class Registry<T1, T2>
    {
        private readonly Dictionary<T1, T2> _registeredTypes;

        protected Registry()
        {
            _registeredTypes = new Dictionary<T1, T2>();
        }

        protected abstract void MeetsRequirements(T1 Key, T2 Value);
        
        public virtual void Add(T1 Key, T2 Value)
        {
            DoAdd(Key, Value);
            _registeredTypes.Add(Key, Value);
        }
        
        protected abstract void DoAdd(T1 Key, T2 Value);

        protected abstract void DoRemove(T1 Key, T2 Value);

        public virtual void Unregister()
        {
            foreach (var pair in _registeredTypes)
            {
                DoRemove(pair.Key, pair.Value);
            }
            _registeredTypes.Clear();
        }
    }
}