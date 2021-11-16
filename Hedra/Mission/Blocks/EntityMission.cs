using System.Numerics;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;

namespace Hedra.Mission.Blocks
{
    public abstract class EntityMission : MissionBlock
    {
        protected EntityMission(params IEntity[] Entities)
        {
            this.Entities = Entities;
        }

        protected IEntity[] Entities { get; }
        protected IEntity Entity => Entities[0];
        public override Vector3 Location => Entities[0].Position;

        public override bool HasLocation => true;

        public override void Setup()
        {
        }

        public override QuestView BuildView()
        {
            return new EntityView((AnimatedUpdatableModel)Entities[0].Model);
        }
    }
}