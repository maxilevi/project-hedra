using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hedra.Engine.ItemSystem
{
    public class ItemPoolSettings
    {
        public ItemTier Tier { get; set; }
        public bool SameTier { get; set; } = false;
        public string EquipmentType { get; set; }
        public Random Rng { get; set; }

        public ItemPoolSettings() { }
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
            this.Rng = Utils.Rng;
        }
    }

    public enum ItemTier : int {
        Common,
        Uncommon,
        Rare,
        Unique,
        Legendary,
        Divine
    }
}
