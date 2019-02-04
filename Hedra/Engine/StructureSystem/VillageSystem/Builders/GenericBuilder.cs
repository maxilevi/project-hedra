using Hedra.Engine.Generation;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class GenericBuilder : LivableBuildingBuilder<GenericParameters>
    {
        public GenericBuilder(CollidableStructure Structure) : base(Structure)
        {
        }
    }
}