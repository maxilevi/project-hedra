using System;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;
using System.Numerics;

namespace Hedra.Engine.StructureSystem.GhostTown
{
    public class TombStructureDesign : SimpleStructureDesign<GhostTownTombstone>
    {
        public override int PlateauRadius => 64;
        public override VertexData Icon => null;
        protected override Vector3 Scale => Vector3.One * 5;
        protected override int StructureChance => StructureGrid.TombstoneChance;
        protected override CacheItem? Cache => CacheItem.Grave;
        
        protected override GhostTownTombstone Create(Vector3 Position, float Size)
        {
            return new GhostTownTombstone(Position);
        }
    }
    
    public class GhostTownTombstone : Tombstone
    {
        public GhostTownTombstone(Vector3 Position) : base(Position)
        {
        }
    }
}