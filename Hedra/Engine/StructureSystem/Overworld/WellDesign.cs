using Hedra.Engine.CacheSystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class WellDesign : QuestGiverStructureDesign<Well>
    {
        public override int PlateauRadius => 80;
        public override VertexData Icon => null;
        protected override float EffectivePlateauRadius => 48;
        protected override int StructureChance => StructureGrid.WellChance;
        protected override CacheItem Cache => CacheItem.Well;
        protected override Vector3 Offset => Vector3.UnitZ * 8f;
        protected override float QuestChance => 0;

        protected override Well Create(Vector3 Position, float Size)
        {
            return new Well(Position, Size);
        }
    }
}