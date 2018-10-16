using Hedra.Engine.Generation;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class NeighbourhoodWellBuilder : Builder<NeighbourhoodParameters>
    {
        public override bool Place(NeighbourhoodParameters Parameters, VillageCache Cache)
        {
            return true;
        }

        public override void Polish(NeighbourhoodParameters Parameters)
        {
            World.AddStructure(new LampPost(Vector3.UnitY * 8f + Parameters.Position)
            {
                Radius = 386,
                LightColor = new Vector3(.25f, .25f, .25f)
            });
        }
    }
}