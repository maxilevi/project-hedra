using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hedra.Engine.ItemSystem.WeaponSystem
{
    public static class WeaponFactory
    {
        private static readonly Dictionary<string, Type> Weapons;

        static WeaponFactory()
        {
            Weapons = new Dictionary<string, Type>();
            Type[] weaponTypes = Reflection.GetLoadableTypes(Assembly.GetExecutingAssembly(), typeof(WeaponFactory).Namespace).ToArray();
            foreach (var weaponType in weaponTypes)
            {
                if(weaponType.IsSubclassOf(typeof(Weapon)))
                    Weapons.Add(weaponType.Name, weaponType);
            }
        }

        public static Weapon Get(Item Item)
        {
            return (Weapon) Activator.CreateInstance(Weapons[Item.WeaponType], Item.Model);
        }
    }
}
