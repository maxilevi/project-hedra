using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class SpawnCampfire : Campfire
    {
        public SpawnCampfire(Vector3 Position) : base(Position)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
            World.StructureHandler.SpawnCampfireSpawned = false;
        }
    }
}