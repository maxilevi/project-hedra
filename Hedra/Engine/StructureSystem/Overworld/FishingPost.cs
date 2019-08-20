using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class FishingPost : BaseStructure
    {
        public IHumanoid Fisherman { get; set; }
        public FishingPost(Vector3 Position) : base(Position)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
            Fisherman?.Dispose();
        }
    }
}