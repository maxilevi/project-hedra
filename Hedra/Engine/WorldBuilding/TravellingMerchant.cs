using System;
using Hedra.Engine.Player;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.EntitySystem;
using System.Numerics;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.QuestSystem;
using Hedra.Mission;

namespace Hedra.Engine.WorldBuilding
{
    /// <summary>
    /// Description of Campfire.
    /// </summary>
    public class TravellingMerchant : Campfire, ICompletableStructure
    {
        public static readonly Vector3 CampfireOffset = Vector3.UnitX * 12f;
        public IHumanoid Merchant { get; }
        public ItemCollect ItemsToBuy { get; }

        public TravellingMerchant(Vector3 Position) : base(Position)
        {
            Merchant = World.WorldBuilding.SpawnHumanoid(HumanType.TravellingMerchant, Position);
        }
        
        protected override Vector3 FirePosition => Position - CampfireOffset;

        public override void Dispose()
        {
            base.Dispose();
            Merchant.Dispose();
        }

        public bool Completed => throw new NotImplementedException();
    }
}
