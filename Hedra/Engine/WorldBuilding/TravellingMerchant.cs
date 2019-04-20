using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.WorldBuilding
{
    /// <summary>
    /// Description of Campfire.
    /// </summary>
    public class TravellingMerchant : Campfire
    {
        public static readonly Vector3 CampfireOffset = Vector3.UnitX * 12f;
        private readonly IHumanoid _merchant;

        public TravellingMerchant(Vector3 Position) : base(Position)
        {
            _merchant = World.WorldBuilding.SpawnHumanoid(HumanType.TravellingMerchant, Position);
        }
        
        protected override Vector3 FirePosition => Position - CampfireOffset;

        public override void Dispose()
        {
            base.Dispose();
            _merchant.Dispose();
        }
    }
}
