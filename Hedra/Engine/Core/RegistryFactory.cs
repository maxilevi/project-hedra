using System;
using System.Collections.Generic;

namespace Hedra.Engine.Core
{
    public abstract class RegistryFactory<T> where T : class, new()
    {
        private static T _instance;
        private readonly Dictionary<string, Type> _table;

        protected RegistryFactory()
        {
            _table = new Dictionary<string, Type>();
        }

        public static T Instance => _instance ?? (_instance = new T());

        public void Register(string Name, Type ClassObject)
        {
            _table.Add(Name, ClassObject);
        }
        
        public void Unregister(string Name)
        {
            _table.Remove(Name);
        }

        protected Type this[string Key] => _table[Key];
    }
}