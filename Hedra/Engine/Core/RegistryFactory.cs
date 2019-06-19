using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedra.Engine.Core
{
    public abstract class RegistryFactory<T1, T2, T3> where T1 : class, new()
    {
        private static T1 _instance;
        private readonly Dictionary<T2, T3> _table;

        protected RegistryFactory()
        {
            _table = new Dictionary<T2, T3>();
        }

        public static T1 Instance => _instance ?? (_instance = new T1());

        public void Register(T2 Name, T3 ClassObject)
        {
            _table.Add(Name, ClassObject);
        }
        
        public void Unregister(T2 Name)
        {
            _table.Remove(Name);
        }

        protected void Clear()
        {
            _table.Clear();
        }

        protected bool Contains(T2 Key)
        {
            return _table.ContainsKey(Key);
        }

        protected T3 this[T2 Key] => _table[Key];

        protected T3[] GetAll() => _table.Values.ToArray();
    }
}