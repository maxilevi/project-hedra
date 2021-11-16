using System.Numerics;
using Hedra.Engine.StructureSystem.Overworld;

namespace Hedra.Structures
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