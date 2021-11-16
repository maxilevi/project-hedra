using System.Collections.Generic;
using System.Numerics;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class SolitaryFisherman : BaseStructure, IQuestStructure
    {
        public SolitaryFisherman(List<BaseStructure> Children, List<IEntity> Npcs) : base(Children, Npcs)
        {
        }

        public SolitaryFisherman(Vector3 Position) : base(Position)
        {
        }

        public IHumanoid NPC { get; set; }
    }
}