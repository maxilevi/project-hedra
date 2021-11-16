using System.Linq;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Behaviours
{
    public class GroupHostileBehaviour : HostileBehaviour
    {
        public GroupHostileBehaviour(IEntity Parent) : base(Parent)
        {
            Attack.TargetChanged += AlertGroup;
        }

        public float CallRadius { get; set; } = 48f;

        protected void AlertGroup(IEntity NewTarget)
        {
            if (NewTarget == null) return;
            var nearEntities = World.InRadius<IEntity>(Parent.Position, CallRadius).Where(E => E.Type == Parent.Type)
                .ToArray();
            for (var i = 0; i < nearEntities.Length; ++i)
            {
                var baseAIComponent = nearEntities[i].SearchComponent<BasicAIComponent>();
                var groupHostile = baseAIComponent.SearchBehaviour<GroupHostileBehaviour>();
                if (groupHostile != null)
                    if (groupHostile.Attack.Target == null)
                        groupHostile.Attack.SetTarget(NewTarget);
            }
        }
    }
}