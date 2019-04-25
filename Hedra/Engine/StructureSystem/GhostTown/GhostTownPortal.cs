using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.StructureSystem.GhostTown
{
    public class GhostTownPortal : Portal
    {
        public IHumanoid NPC { get; set; }
        
        public GhostTownPortal(Vector3 Position, Vector3 Scale, int Index, Vector3 DefaultSpawn) : base(Position, Scale, Index, DefaultSpawn)
        {
        }
        
        protected GhostTownPortal(Vector3 Position, Vector3 Scale, int Index) : base(Position, Scale, Index)
        {
        }
        

        public override void Dispose()
        {
            base.Dispose();
            NPC?.Dispose();
        }
    }
}