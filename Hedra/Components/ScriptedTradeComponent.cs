using System;
using System.Collections.Generic;
using Hedra.Core;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Scripting;
using Hedra.EntitySystem;
using Hedra.Numerics;

namespace Hedra.Components
{
    public abstract class ScriptedTradeComponent : TradeComponent
    {
        private static readonly Script Script = Interpreter.GetScript("Trade.py");

        protected ScriptedTradeComponent(IHumanoid Parent) : base(Parent)
        {
        }

        protected abstract string BuildInventoryFunctionName { get; }

        public override Dictionary<int, Item> BuildInventory()
        {
            var rng = new Random(World.Seed + Unique.GenerateSeed(Parent.Position.Xz()));
            var dict = new Dictionary<int, Item>();
            Script.Get(BuildInventoryFunctionName).Invoke(dict, TradeInventory.MerchantSpaces, rng);
            return dict;
        }
    }
}