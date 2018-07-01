using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hedra.Engine.ItemSystem.ArmorSystem
{
    internal static class ArmorFactory
    {
        private static readonly Dictionary<string, Type> Armors;

        static ArmorFactory()
        {
            Armors = new Dictionary<string, Type>();
            Type[] weaponTypes = Assembly.GetExecutingAssembly().GetLoadableTypes(typeof(ArmorFactory).Namespace).ToArray();
            foreach (var weaponType in weaponTypes)
            {
                if (weaponType.IsSubclassOf(typeof(ArmorPiece)))
                    Armors.Add(weaponType.Name, weaponType);
            }
        }

        public static bool Contains(Item Item)
        {
            return Item.EquipmentType != null && Armors.ContainsKey(Item.EquipmentType);
        }

        public static ArmorPiece Get(Item Item)
        {
            var weapon = (ArmorPiece)Activator.CreateInstance(Armors[Item.EquipmentType], Item.Model);
            return weapon;
        }
    }
}
