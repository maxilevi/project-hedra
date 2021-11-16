using System;
using Hedra.Core;
using Hedra.Items;

namespace Hedra.Engine.ItemSystem
{
    public class ItemPoolSettings
    {
        public ItemPoolSettings(ItemTier Tier)
        {
            Initialize(Tier, null);
        }

        public ItemPoolSettings(ItemTier Tier, EquipmentType Type)
        {
            Initialize(Tier, Type.ToString());
        }

        public ItemPoolSettings(ItemTier Tier, string WeaponType)
        {
            Initialize(Tier, WeaponType);
        }

        public ItemPoolSettings(string Tier, string WeaponType)
        {
            Initialize((ItemTier)Enum.Parse(typeof(ItemTier), Tier), WeaponType);
        }

        public ItemTier Tier { get; set; }
        public bool RandomizeTier { get; set; } = true;
        public bool SameTier { get; set; } = true;
        public string EquipmentType { get; set; }
        public int Seed { get; set; }

        private void Initialize(ItemTier Tier, string WeaponType)
        {
            this.Tier = Tier;
            EquipmentType = WeaponType;
            Seed = Unique.RandomSeed();
        }
    }
}