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
    public class MerchantComponent : ScriptedTradeComponent
    {
        public MerchantComponent(IHumanoid Parent) : base(Parent)
        {
        }

        protected override string BuildInventoryFunctionName => "build_merchant_inventory";
    }
}
