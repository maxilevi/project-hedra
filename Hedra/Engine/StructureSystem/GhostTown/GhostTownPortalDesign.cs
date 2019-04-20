using Hedra.Engine.CacheSystem;
using Hedra.Engine.Player;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.GhostTown
{
    public class GhostTownPortalDesign : SimpleStructureDesign<GhostTownPortal>
    {
        public override int PlateauRadius => 180;
        public override VertexData Icon { get; } = CacheManager.GetModel(CacheItem.PortalIcon);
        protected override int StructureChance => StructureGrid.GhostTownPortalChance;
        protected override CacheItem Cache => CacheItem.Portal;
        protected override Vector3 Scale => Vector3.One * 10;

        protected override GhostTownPortal Create(Vector3 Position, float Size)
        {
            return new GhostTownPortal(Position, Scale, RealmHandler.GhostTown);
        }
    }
}