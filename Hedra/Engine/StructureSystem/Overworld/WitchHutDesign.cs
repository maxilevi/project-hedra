using Hedra.Engine.CacheSystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class WitchHutDesign : SimpleStructureDesign<WitchHut>
    {
        public override int PlateauRadius => 128;
        public override VertexData Icon { get; } = CacheManager.GetModel(CacheItem.WitchHutIcon);
        protected override int StructureChance => StructureGrid.WitchHut;
        protected override CacheItem? Cache { get; }
        protected override WitchHut Create(Vector3 Position, float Size)
        {
            throw new System.NotImplementedException();
        }
    }
}