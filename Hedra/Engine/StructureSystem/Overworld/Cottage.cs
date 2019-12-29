using System.Collections.Generic;
using System.Numerics;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public abstract class Cottage : BaseStructure, IQuestStructure
    {
        protected float Radius { get; }
        protected Cottage(List<BaseStructure> Children, List<IEntity> Npcs) : base(Children, Npcs)
        {
        }

        protected Cottage(Vector3 Position) : base(Position)
        {
        }
        
        protected Cottage(Vector3 Position, float Radius) : base(Position)
        {
            this.Radius = Radius;
        }

        public IHumanoid NPC { get; set; }
        
        public override void Dispose()
        {
            base.Dispose();
            NPC?.Dispose();
        }
    }
}