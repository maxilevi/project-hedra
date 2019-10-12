using Hedra.EntitySystem;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class SpawnCampfire : Campfire
    {
        public IHumanoid Villager { get; set; }
        
        public SpawnCampfire(Vector3 Position) : base(Position)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
            Villager.Dispose();
            SpawnCampfireDesign.Spawned  = false;
        }
    }
}