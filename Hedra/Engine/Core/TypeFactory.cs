using System;

namespace Hedra.Engine.Core
{
    public abstract class TypeFactory<T> : RegistryFactory<T, string, Type> where T : class, new()
    {        
    }
}