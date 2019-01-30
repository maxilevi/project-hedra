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
using Hedra.Engine.Core;
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
            var rng = new Random(World.Seed + Unique.GenerateSeed(Parent.BlockPosition.Xz));
            var berry = ItemPool.Grab(ItemType.Berry);
            berry.SetAttribute(CommonAttributes.Amount, int.MaxValue);
            var flask = ItemPool.Grab(ItemType.GlassFlask);
            flask.SetAttribute(CommonAttributes.Amount, int.MaxValue);
            var bowl = ItemPool.Grab(ItemType.WoodenBowl);
            bowl.SetAttribute(CommonAttributes.Amount, int.MaxValue);
            var stoneArrow = ItemPool.Grab(ItemType.StoneArrow);
            stoneArrow.SetAttribute(CommonAttributes.Amount, int.MaxValue);
            var recipes = new[]
            {
                ItemPool.Grab(ItemType.PumpkinPieRecipe),
                ItemPool.Grab(ItemType.CookedMeatRecipe),
                ItemPool.Grab(ItemType.HealthPotionRecipe),
                ItemPool.Grab(ItemType.CornSoupRecipe),
            };
            var newItems = new Dictionary<int, Item>
            {
                {TradeInventory.MerchantSpaces - 1, berry},
                {TradeInventory.MerchantSpaces - 2, flask},
                {TradeInventory.MerchantSpaces - 3, bowl},
                //{TradeInventory.MerchantSpaces - 4, rng.NextBool() ? stoneArrow : null},
                {TradeInventory.MerchantSpaces - 5, null /*recipes[rng.Next(0, recipes.Length)]*/},
            };
            if (_isTravellingMerchant)
            {
                newItems.Add(0, ItemPool.Grab("HorseMount"));
                newItems.Add(2, ItemPool.Grab(ItemType.Boat));
            }
            return newItems;
        }
    }
}
