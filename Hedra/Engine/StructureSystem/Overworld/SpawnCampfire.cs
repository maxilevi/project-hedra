using Hedra.Engine.QuestSystem;
using Hedra.EntitySystem;
using OpenTK;

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
            SpawnCampfireDesign.Spawned  = false;
        }
    }
}