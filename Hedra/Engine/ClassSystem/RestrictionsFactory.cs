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
    }
}