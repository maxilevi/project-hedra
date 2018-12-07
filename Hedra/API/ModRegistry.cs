using System;
using System.Collections.Generic;

namespace Hedra.API
{
    public abstract class ModRegistry
    {
        private readonly Dictionary<string, Type> _registeredTypes;

        protected ModRegistry()
        {
            _registeredTypes = new Dictionary<string, Type>();
        }

        public void Add(string Name, Type ClassType)
        {
            if (!ClassType.IsSubclassOf(RegistryType) || ClassType.IsAbstract)
                throw new ArgumentOutOfRangeException($"{ClassType.FullName} needs to inherit from {RegistryType.FullName} or be concrete.");
            DoAdd(Name, ClassType);
            _registeredTypes.Add(Name, ClassType);
        }
        
        protected abstract void DoAdd(string Name, Type ClassType);

        protected abstract void DoRemove(string Name);
        
        protected abstract Type RegistryType { get; }

        public void Unregister()
        {
            foreach (var pair in _registeredTypes)
            {
                DoRemove(pair.Key);
            }
            _registeredTypes.Clear();
        }
    }
}