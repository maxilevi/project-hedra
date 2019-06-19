using System;
using Hedra.Engine.Core;

namespace Hedra.Engine.ClassSystem
{
    public class RestrictionsFactory : RegistryFactory<RestrictionsFactory, Type, string[]>
    {
        public string[] Build(Type Class)
        {
            return Contains(Class) 
                ? this[Class]
                : new string[0];
        }

        public bool Has(Type Key)
        {
            return Contains(Key);
        }

        public string[] Get(Type Key)
        {
            return this[Key];
        }

        public void Clear()
        {
            base.Clear();
        }
    }
}