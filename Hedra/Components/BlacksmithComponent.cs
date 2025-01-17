using System;
using System.Collections.Generic;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Items;

namespace Hedra.Components
{
    public class BlacksmithComponent : TradeComponent
    {
        public BlacksmithComponent(Humanoid Parent) : base(Parent)
        {
        }

        public override Dictionary<int, Item> BuildInventory()
        {
            var weapons = new[]
            {
                EquipmentType.Axe,
                EquipmentType.Sword,
                EquipmentType.Hammer,
                EquipmentType.Ring,
                EquipmentType.Bow,
                EquipmentType.Claw,
                EquipmentType.Katar,
                EquipmentType.Knife,
                EquipmentType.Staff,
                EquipmentType.DoubleBlades
            };
            var rng = new Random();
            return new Dictionary<int, Item>
            {
                { 0, ItemPool.Grab(new ItemPoolSettings(ItemTier.Rare, weapons[rng.Next(0, weapons.Length)])) },
                { 1, ItemPool.Grab(new ItemPoolSettings(ItemTier.Rare, weapons[rng.Next(0, weapons.Length)])) },
                { 2, ItemPool.Grab(new ItemPoolSettings(ItemTier.Rare, weapons[rng.Next(0, weapons.Length)])) },
                { 3, ItemPool.Grab(new ItemPoolSettings(ItemTier.Rare, weapons[rng.Next(0, weapons.Length)])) }
                /*{TradeInventory.MerchantSpaces - 1, ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, EquipmentType.Helmet))},
                {TradeInventory.MerchantSpaces - 2, ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, EquipmentType.Chestplate))},
                {TradeInventory.MerchantSpaces - 3, ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, EquipmentType.Pants))},
                {TradeInventory.MerchantSpaces - 4, ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, EquipmentType.Boots))},*/
            };
        }
    }
}