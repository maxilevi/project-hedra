using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.StructureSystem;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.WorldBuilding
{
    /// <summary>
    /// Description of Campfire.
    /// </summary>
    public class TravellingMerchant : Campfire
    {
        private readonly IHumanoid _merchant;
        protected override bool CanUseForCrafting => false;

        public TravellingMerchant(Vector3 Position) : base(Position)
        {
            _merchant = World.WorldBuilding.SpawnHumanoid(HumanType.TravellingMerchant, Position + Vector3.UnitX * 12f);
        }

        public override void Dispose()
        {
            base.Dispose();
            _merchant.Dispose();
        }
    }
}
