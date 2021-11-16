using System.Numerics;
using Hedra.EntitySystem;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class SpawnCampfire : Campfire
    {
        public SpawnCampfire(Vector3 Position) : base(Position)
        {
        }

        public IHumanoid Villager { get; set; }

        public override void Dispose()
        {
            base.Dispose();
            Villager.Dispose();
            SpawnCampfireDesign.Spawned = false;
        }
    }
}