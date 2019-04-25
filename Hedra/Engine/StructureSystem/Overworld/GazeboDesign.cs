using Hedra.Engine.CacheSystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class GazeboDesign : QuestGiverStructureDesign<Gazebo>
    {
        public override int PlateauRadius => 160;
        public override VertexData Icon { get; } = CacheManager.GetModel(CacheItem.GazeboIcon);
        protected override float EffectivePlateauRadius => 80;
        protected override int StructureChance => StructureGrid.GazeboChance;
        protected override CacheItem? Cache => CacheItem.Gazebo;
        protected override Vector3 DefaultLookingDirection => Vector3.UnitX;
        protected override Vector3 Offset => GazeboCache.OffsetFromGround;
        protected override float QuestChance => 1;
        protected override Gazebo Create(Vector3 Position, float Size)
        {
            return new Gazebo(Position, Size);
        }
    }
}