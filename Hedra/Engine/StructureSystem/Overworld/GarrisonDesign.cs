using System;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class GarrisonDesign : SimpleStructureDesign<Garrison>
    {
        public override int PlateauRadius => 384;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.GarrisonIcon);
        public override bool CanSpawnInside => false;
        protected override int StructureChance => StructureGrid.GarrisonChance;
        protected override CacheItem? Cache => CacheItem.Garrison;
        protected override Garrison Create(Vector3 Position, float Size)
        {
            return new Garrison(Position);
        }
    }
}