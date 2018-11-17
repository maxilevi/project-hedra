using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hedra.Engine.ItemSystem.ArmorSystem
{
    public static class ArmorFactory
    {
        private static readonly Dictionary<string, Type> Armors;

        static ArmorFactory()
        {
            Armors = new Dictionary<string, Type>()
            {
                {"Helmet", typeof(HelmetPiece)},
                {"Chestplate", typeof(ChestPiece)},
                {"Pants", typeof(PantsPiece)},
                {"Boots", typeof(BootsPiece)},
            };
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
