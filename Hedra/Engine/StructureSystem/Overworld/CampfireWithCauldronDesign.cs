using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class CampfireWithCauldronDesign : CampfireDesign
    {
        public override VertexData Icon => CacheManager.GetModel(CacheItem.CauldronIcon);
        public override bool CanSpawnInside => false;

        public override void Build(CollidableStructure Structure)
        {
            base.Build(Structure);
            var model = DynamicCache.Get("")
        }
    }
}