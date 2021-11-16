using System.Collections.Generic;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.Inventory;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Localization;
using Hedra.Numerics;
using SixLabors.ImageSharp;

namespace Hedra.Components
{
    public delegate void OnItemBought(Item Item);

    public abstract class TradeComponent : Component<IHumanoid>
    {
        private const int TradeRadius = 12;
        private Dictionary<int, Item> _originalItems;

        protected TradeComponent(IHumanoid Parent) : base(Parent)
        {
            this.Parent.Gold = int.MaxValue;
        }

        protected int MerchantSpaces => TradeInventory.MerchantSpaces;
        public Dictionary<int, Item> Items { get; private set; }
        public event OnItemBought ItemBought;

        public abstract Dictionary<int, Item> BuildInventory();

        public void TransactionComplete(Item Item, TransactionType Type)
        {
            Items = new Dictionary<int, Item>(_originalItems);
            if (Type == TransactionType.Buy)
                ItemBought?.Invoke(Item);
        }

        public override void Update()
        {
            if (_originalItems == null)
            {
                _originalItems = BuildInventory();
                Items = new Dictionary<int, Item>(_originalItems);
            }

            var player = LocalPlayer.Instance;

            if ((LocalPlayer.Instance.Position - Parent.Position).Xz().LengthSquared() < TradeRadius * TradeRadius)
            {
                Parent.Orientation =
                    (LocalPlayer.Instance.Position - Parent.Position).Xz().NormalizedFast().ToVector3();
                Parent.Model.TargetRotation = Physics.DirectionToEuler(Parent.Orientation);
            }

            var canTrade = player.CanInteract && !player.IsDead && !GameSettings.Paused &&
                           !player.InterfaceOpened;

            bool InRadiusFunc()
            {
                return (player.Position - Parent.Position).LengthSquared() <
                    TradeInventory.TradeRadius * TradeInventory.TradeRadius && !player.Trade.IsTrading;
            }

            var inRadius = InRadiusFunc();

            if (!canTrade || !inRadius) return;

            player.MessageDispatcher.ShowMessageWhile(Translations.Get("to_trade", Controls.Interact), Color.White,
                InRadiusFunc);
        }
    }
}