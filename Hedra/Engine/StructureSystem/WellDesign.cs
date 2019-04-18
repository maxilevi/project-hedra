using System;
using System.Linq;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Generation;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class WellDesign : QuestGiverStructureDesign<Well>
    {
        public override int PlateauRadius => 80;
        public override VertexData Icon => null;
        protected override float EffectivePlateauRadius => 48;
        protected override int StructureChance => StructureGrid.WellChance;
        protected override CacheItem Cache => CacheItem.Well;
        protected override Vector3 Offset => Vector3.UnitZ * 8f;
        protected override float QuestChance => .33f;

        protected override Well Create(Vector3 Position, float Size)
        {
            return new Well(Position, Size);
        }
    }
}