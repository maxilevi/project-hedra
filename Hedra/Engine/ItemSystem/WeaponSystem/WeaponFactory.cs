using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hedra.Engine.ItemSystem.WeaponSystem
{
    internal static class WeaponFactory
    {
        private static readonly Dictionary<string, Type> Weapons;
        private static readonly Dictionary<Item, Weapon> WeaponCache;

        static WeaponFactory()
        {
            WeaponCache = new Dictionary<Item, Weapon>();
            Weapons = new Dictionary<string, Type>();
            Type[] weaponTypes = Assembly.GetExecutingAssembly().GetLoadableTypes(typeof(WeaponFactory).Namespace).ToArray();
            foreach (var weaponType in weaponTypes)
            {
                if(weaponType.IsSubclassOf(typeof(Weapon)))
                    Weapons.Add(weaponType.Name, weaponType);
            }
        }

        public static bool Contains(Item Item)
        {
            return Item.EquipmentType != null && Weapons.ContainsKey(Item.EquipmentType);
        }

        public static Weapon Get(Item Item)
        {
            var weapon = (Weapon) Activator.CreateInstance(Weapons[Item.EquipmentType], Item.Model);
            weapon.Describer = EffectDescriber.FromItem(Item);
            return weapon;
        }
    }
}
