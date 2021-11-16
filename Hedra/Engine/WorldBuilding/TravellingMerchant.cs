using System.Numerics;
using Hedra.Components;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.EntitySystem;
using Hedra.Mission;

namespace Hedra.Engine.WorldBuilding
{
    /// <summary>
    ///     Description of Campfire.
    /// </summary>
    public class TravellingMerchant : Campfire, ICompletableStructure
    {
        public static readonly Vector3 CampfireOffset = Vector3.UnitX * 12f;

        public TravellingMerchant(Vector3 Position) : base(Position)
        {
            Merchant = NPCCreator.SpawnHumanoid(HumanType.TravellingMerchant, Position);
            Merchant.SearchComponent<TradeComponent>().ItemBought += OnItemBought;
        }

        public IHumanoid Merchant { get; }
        public ItemCollect ItemsToBuy { get; set; }

        protected override Vector3 FirePosition => Position - CampfireOffset;

        public override void Dispose()
        {
            base.Dispose();
            Merchant.Dispose();
        }

        public bool Completed => ItemsToBuy != null && ItemsToBuy.Amount == 0;

        private void OnItemBought(Item I)
        {
            if (ItemsToBuy == null) return;
            if (ItemsToBuy.Name == I.Name && ItemsToBuy.Amount > 0) ItemsToBuy.Amount -= 1;
        }
    }
}