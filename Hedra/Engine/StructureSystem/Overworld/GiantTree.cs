using System.Numerics;
using Hedra.Engine.QuestSystem;
using Hedra.EntitySystem;
using Hedra.Mission;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public sealed class GiantTree : BaseStructureWithChest, ICompletableStructure
    {
        public GiantTree(Vector3 Position) : base(Position)
        {
        }

        public IEntity Boss { get; set; }

        public QuestReward Reward => null;

        public bool Completed => Boss?.IsDead ?? false;

        public override void Dispose()
        {
            Boss?.Dispose();
            base.Dispose();
        }
    }
}