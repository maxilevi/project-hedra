/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/07/2017
 * Time: 02:20 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.Inventory;

namespace Hedra.Components
{
    /// <inheritdoc />
    /// <summary>
    /// Description of MerchantComponent.
    /// </summary>
    public class MerchantComponent : TradeComponent
    {
        private readonly bool _isTravellingMerchant;

        public MerchantComponent(Humanoid Parent, bool TravellingMerchant) : base(Parent)
        {
            _isTravellingMerchant = TravellingMerchant;
        }

        public override Dictionary<int, Item> BuildInventory()
        {
            var rng = new Random(World.Seed + 82823 + Utils.Rng.Next(-9999999, 9999999));
            var items = new[]
            {
                ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, EquipmentType.Axe)),
                ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, EquipmentType.Sword)),
                ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, EquipmentType.Hammer)),
                ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, EquipmentType.Claw)),
                ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, EquipmentType.Katar)),
                ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, EquipmentType.DoubleBlades)),
                ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, EquipmentType.Bow)),
                ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, EquipmentType.Knife))
            };
            var berry = ItemPool.Grab(ItemType.Berry);
            berry.SetAttribute(CommonAttributes.Amount, int.MaxValue);
            var newItems = new Dictionary<int, Item>
            {
                {TradeInventory.MerchantSpaces - 1, berry}
            };
            for (var i = 0; i < 4; i++)
            {
                newItems.Add(i, items[rng.Next(0, items.Length)]);
            }

            if (_isTravellingMerchant)
            {
                newItems.Add(TradeInventory.MerchantSpaces - 2, ItemPool.Grab("HorseMount"));
                //newItems.Add(TradeInventory.MerchantSpaces - 3, ItemPool.Grab("WolfMount"));
                newItems.Add(TradeInventory.MerchantSpaces - 3, ItemPool.Grab(ItemType.Glider));
                newItems.Add(TradeInventory.MerchantSpaces - 4, ItemPool.Grab(ItemType.Boat));
            }
            return newItems;
        }
    }
}
