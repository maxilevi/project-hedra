using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Mission;
using Hedra.Rendering;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.StructureSystem.GhostTown
{
    public class GhostTownPortal : Portal, ICompletableStructure
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

        public bool Completed => TeleportedRecently;
    }
}