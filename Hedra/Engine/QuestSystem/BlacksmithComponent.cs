using System.Collections.Generic;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;

namespace Hedra.Engine.QuestSystem
{
    internal class BlacksmithComponent :  TradeComponent
    {
        public BlacksmithComponent(Humanoid Parent) : base(Parent)
        {
        }

        public override Dictionary<int, Item> BuildInventory()
        {
            return new Dictionary<int, Item>
            {
                {0, ItemPool.Grab(new ItemPoolSettings(ItemTier.Rare, EquipmentType.Axe))},
                {1, ItemPool.Grab(new ItemPoolSettings(ItemTier.Rare, EquipmentType.Sword))},
                {2, ItemPool.Grab(new ItemPoolSettings(ItemTier.Rare, EquipmentType.Hammer))},
                {3, ItemPool.Grab(new ItemPoolSettings(ItemTier.Rare, EquipmentType.Ring))},
                /*{TradeInventory.MerchantSpaces - 1, ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, EquipmentType.Helmet))},
                {TradeInventory.MerchantSpaces - 2, ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, EquipmentType.Chestplate))},
                {TradeInventory.MerchantSpaces - 3, ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, EquipmentType.Pants))},
                {TradeInventory.MerchantSpaces - 4, ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, EquipmentType.Boots))},*/
            };
        }
    }
}