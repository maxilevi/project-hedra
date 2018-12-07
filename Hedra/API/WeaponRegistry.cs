using System;
using Hedra.WeaponSystem;

namespace Hedra.API
{
    public class WeaponRegistry : ModRegistry
    {
        protected override void DoAdd(string Name, Type ClassType)
        {
            WeaponFactory.Register(Name, ClassType);
        }

        protected override void DoRemove(string Name)
        {
            WeaponFactory.Unregister(Name);
        }

        protected override Type RegistryType { get; } = typeof(Weapon);
    }
}