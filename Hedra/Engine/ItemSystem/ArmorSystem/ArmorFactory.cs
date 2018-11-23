using System;
using System.Collections.Generic;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Animation.ColladaParser;

namespace Hedra.Engine.ItemSystem.ArmorSystem
{
    public static class ArmorFactory
    {
        private static readonly Dictionary<string, Type> Armors;

        static ArmorFactory()
        {
            Armors = new Dictionary<string, Type>()
            {
                {"Chestplate", typeof(ChestPiece)},
            };
        }

        public static bool Contains(Item Item)
        {
            return Item.EquipmentType != null && Armors.ContainsKey(Item.EquipmentType);
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
