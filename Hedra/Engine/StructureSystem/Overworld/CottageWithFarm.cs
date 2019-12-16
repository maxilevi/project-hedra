using System.Collections.Generic;
using System.Numerics;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class CottageWithFarm : BaseStructure, IQuestStructure
    {
        public CottageWithFarm(Vector3 Position) : base(Position)
        {
        }

        public IHumanoid NPC { get; set; }

        public override void Dispose()
        {
            base.Dispose();
            NPC?.Dispose();
        }
    }
}