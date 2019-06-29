using System.Collections.Generic;
using System.Drawing;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Game;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.Inventory;
using Hedra.Game;
using Hedra.Items;
using Hedra.Localization;

namespace Hedra.Components
{
    public abstract class TradeComponent : EntityComponent
    {
        protected int MerchantSpaces => TradeInventory.MerchantSpaces;
        private const int TradeRadius = 12;
        public new Humanoid Parent;
        public Dictionary<int, Item> Items { get; private set; }
        private Dictionary<int, Item> _originalItems;

        protected TradeComponent(Humanoid Parent) : base(Parent)
        {
            this.Parent = Parent;
            this.Parent.Gold = int.MaxValue;
        }

        public abstract Dictionary<int, Item> BuildInventory();

        public void TransactionComplete()
        {
            Items = new Dictionary<int, Item>(_originalItems);
        }

        public override void Update()
        {
            if (_originalItems == null)
            {
                _originalItems = this.BuildInventory();
                Items = new Dictionary<int, Item>(_originalItems);
            }
            var player = LocalPlayer.Instance;

            if ((LocalPlayer.Instance.Position - this.Parent.Position).Xz.LengthSquared < TradeRadius * TradeRadius)
            {
                Parent.Orientation = (LocalPlayer.Instance.Position - Parent.Position).Xz.NormalizedFast().ToVector3();
                Parent.Model.TargetRotation = Physics.DirectionToEuler(Parent.Orientation);
            }

            var canTrade = player.CanInteract && !player.IsDead && !GameSettings.Paused &&
                           !player.InterfaceOpened;
            bool InRadiusFunc() => (player.Position - Parent.Position).LengthSquared < TradeInventory.TradeRadius * TradeInventory.TradeRadius && !player.Trade.IsTrading;

            var inRadius = InRadiusFunc();

            if (!canTrade || !inRadius) return;

            player.MessageDispatcher.ShowMessageWhile(Translations.Get("to_trade", Controls.Interact), Color.White, InRadiusFunc);
        }
    }
}
