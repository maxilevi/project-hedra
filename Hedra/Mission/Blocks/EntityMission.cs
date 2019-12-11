using System.Numerics;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;

namespace Hedra.Mission.Blocks
{
    public abstract class EntityMission : MissionBlock
    {
        public IEntity Entity { get; }
        
        protected EntityMission(IEntity Entity)
        {
            this.Entity = Entity;
        }
        
        public override void Setup()
        {
        }

        public override QuestView BuildView()
        {
            return new EntityView((AnimatedUpdatableModel)Entity.Model);
        }
        public override Vector3 Location => Entity.Position;

        public override bool HasLocation => true;
    }
}