using System.Numerics;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.GhostTown
{
    public class TombStructureDesign : SimpleStructureDesign<GhostTownTombstone>
    {
        public override int PlateauRadius => 64;
        public override VertexData Icon => null;
        protected override Vector3 StructureScale => Vector3.One * 5;
        protected override int StructureChance => StructureGrid.TombstoneChance;
        protected override CacheItem? Cache => CacheItem.Grave;
        public override bool CanSpawnInside => true;

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