using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.ItemSystem;

namespace Hedra.WeaponSystem
{
    public static class WeaponFactory
    {
        private static readonly Dictionary<string, Type> Weapons = new Dictionary<string, Type>();

        public static void Register(string Name, Type Weapon)
        {
            Weapons.Add(Name, Weapon);
        }

        public static void Unregister(string Name)
        {
            Weapons.Remove(Name);
        }

        public static bool Contains(Item Item)
        {
            return Contains(Item.EquipmentType);
        }

        public static bool Contains(string EquipmentType)
        {
            return EquipmentType != null && Weapons.ContainsKey(EquipmentType);
        }

        public static Weapon Get(Item Item)
        {
            var type = Weapons[Item.EquipmentType];
            var weapon = (Weapon)Activator.CreateInstance(type, Item.Model);
            weapon.Describer = EffectDescriber.FromItem(Item);
            return weapon;
        }

        public static Type[] GetTypes()
        {
            return Weapons.Values.ToArray();
        }
    }
}