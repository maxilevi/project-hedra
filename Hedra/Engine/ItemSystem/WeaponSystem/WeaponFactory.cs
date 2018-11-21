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
            Weapons = new Dictionary<string, Type>()
            {
                {"Sword", typeof(Sword)},
                {"Axe", typeof(Axe)},
                {"Hammer", typeof(Hammer)},
                {"Claw", typeof(Claw)},
                {"Katar", typeof(Katar)},
                {"DoubleBlades", typeof(DoubleBlades)},
                {"Knife", typeof(Knife)},
                {"Bow", typeof(Bow)},
                {"Staff", typeof(Staff)}
            };
        }

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
            return Item.EquipmentType != null && Weapons.ContainsKey(Item.EquipmentType);
        }

        public static Weapon Get(Item Item)
        {
            var weapon = (Weapon) Activator.CreateInstance(Weapons[Item.EquipmentType], Item.Model);
            weapon.Describer = EffectDescriber.FromItem(Item);
            return weapon;
        }

        public static Type[] GetTypes()
        {
            return Weapons.Values.ToArray();
        }
    }
}
