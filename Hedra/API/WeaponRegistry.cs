using System;
using Hedra.WeaponSystem;

namespace Hedra.API
{
    public class WeaponRegistry : TypeRegistry
    {
        protected override Type RegistryType { get; } = typeof(Weapon);

        protected override void DoAdd(string Name, Type ClassType)
        {
            WeaponFactory.Register(Name, ClassType);
        }

        protected override void DoRemove(string Name, Type ClassType)
        {
            WeaponFactory.Unregister(Name);
        }
    }
}