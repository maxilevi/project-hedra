using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class SpawnVillage : Village
    {
        public SpawnVillage(Vector3 Position) : base(Position)
        {
        }

        public override void Dispose()
        {
            World.StructureHandler.SpawnVillageSpawned = false;
            base.Dispose();
        }
    }
}