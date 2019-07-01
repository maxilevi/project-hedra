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
using Hedra.Core;
using Hedra.Engine.Core;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Scripting;
using Hedra.EntitySystem;
using Hedra.Items;

namespace Hedra.Components
{
    /// <inheritdoc />
    /// <summary>
    /// Description of MerchantComponent.
    /// </summary>
    public class MerchantComponent : TradeComponent
    {
        private static readonly Script Script = Interpreter.GetScript("Merchant.py");
        private readonly bool _isTravellingMerchant;

        public MerchantComponent(Humanoid Parent, bool TravellingMerchant) : base(Parent)
        {
            _isTravellingMerchant = TravellingMerchant;
        }

        public override Dictionary<int, Item> BuildInventory()
        {
            var rng = new Random(World.Seed + Unique.GenerateSeed(Parent.Position.Xz));
            var dict = new Dictionary<int, Item>();
            Script.Get("build_inventory").Invoke(dict, _isTravellingMerchant, TradeInventory.MerchantSpaces, rng);
            return dict;
        }
    }
}
