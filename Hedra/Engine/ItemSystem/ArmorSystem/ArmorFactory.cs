using System;
using System.Collections.Generic;
using Hedra.Engine.Management;

namespace Hedra.Engine.ItemSystem.ArmorSystem
{
    public static class ArmorFactory
    {
        private static readonly Dictionary<string, Type> Armors;

        static ArmorFactory()
        {
            Armors = new Dictionary<string, Type>
            {
                { "Chestplate", typeof(ChestPiece) },
                { "Helmet", typeof(HelmetPiece) },
                { "Pants", typeof(PantsPiece) },
                { "Boots", typeof(BootsPiece) }
            };
        }

        public static bool Contains(Item Item)
        {
            return Contains(Item.EquipmentType);
        }

        public static bool Contains(string Equipment)
        {
            return Equipment != null && Armors.ContainsKey(Equipment);
        }

        public static ArmorPiece Get(Item Item)
        {
            var armor =
                (ArmorPiece)Activator.CreateInstance(
                    Armors[Item.EquipmentType],
                    AssetManager.DAELoader(Item.ModelTemplate.Path)
                );
            return armor;
        }
    }
}