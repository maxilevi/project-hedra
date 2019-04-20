using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class SpawnVillage : Village
    {
        public SpawnVillage(Vector3 Position) : base(Position)
        {
        }

        public override void Dispose()
        {
            SpawnVillageDesign.Spawned = false;
            base.Dispose();
        }
    }
}