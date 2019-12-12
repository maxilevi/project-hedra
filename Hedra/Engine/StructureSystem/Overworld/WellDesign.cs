using Hedra.Engine.CacheSystem;
using Hedra.Engine.Localization;
using Hedra.Rendering;
using System.Numerics;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class WellDesign : QuestGiverStructureDesign<Well>
    {
        public override int PlateauRadius => 80;
        public override VertexData Icon => null;
        protected override float EffectivePlateauRadius => 48;
        protected override int StructureChance => StructureGrid.WellChance;
        protected override CacheItem? Cache => CacheItem.Well;
        protected override Vector3 Offset => Vector3.UnitZ * 8f;
        protected override Vector3 StructureOffset => -.5f * Vector3.UnitY;
        protected override float QuestChance => 1.0f;
        public override bool CanSpawnInside => true;

        protected override Well Create(Vector3 Position, float Size)
        {
            return new Well(Position, Size);
        }
    }
}