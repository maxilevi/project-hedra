using System.Numerics;
using Hedra.Engine.QuestSystem;
using Hedra.EntitySystem;

namespace Hedra.Engine.StructureSystem.GhostTown
{
    public class GhostTownPortal : Portal, ICompletableStructure
    {
        public GhostTownPortal(Vector3 Position, Vector3 Scale, int Index, Vector3 DefaultSpawn) : base(Position, Scale,
            Index, DefaultSpawn)
        {
        }

        protected GhostTownPortal(Vector3 Position, Vector3 Scale, int Index) : base(Position, Scale, Index)
        {
        }

        public IHumanoid NPC { get; set; }


        public override void Dispose()
        {
            base.Dispose();
            NPC?.Dispose();
        }

        public bool Completed => TeleportedRecently;
    }
}