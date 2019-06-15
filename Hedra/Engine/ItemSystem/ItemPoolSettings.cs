using System;
using Hedra.Engine.Core;

namespace Hedra.Engine.ItemSystem
{
    public class ItemPoolSettings
    {
        public ItemTier Tier { get; set; }
        public bool RandomizeTier { get; set; }
        public bool SameTier { get; set; } = true;
        public string EquipmentType { get; set; }
        public int Seed { get; set; }

        public ItemPoolSettings(ItemTier Tier)
        {
            this.Initialize(Tier, null);
        }
        public ItemPoolSettings(ItemTier Tier, EquipmentType Type)
        {
            this.Initialize(Tier, Type.ToString());
        }

        public ItemPoolSettings(ItemTier Tier, string WeaponType)
        {
            this.Initialize(Tier, WeaponType);
        }

        public ItemPoolSettings(string Tier, string WeaponType)
        {
            this.Initialize((ItemTier) Enum.Parse(typeof(ItemTier),Tier), WeaponType);
        }

        private void Initialize(ItemTier Tier, string WeaponType)
        {
            this.Tier = Tier;
            this.EquipmentType = WeaponType;
            this.Seed = Unique.RandomSeed();
        }
    }

    public enum ItemTier
    {
        Common,
        Uncommon,
        Rare,
        Unique,
        Legendary,
        Divine,
        Misc
    }
}
