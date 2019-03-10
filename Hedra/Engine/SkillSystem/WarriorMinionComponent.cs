using System;
using Hedra.AISystem.Behaviours;
using Hedra.AISystem.Humanoid;
using Hedra.EntitySystem;

namespace Hedra.Engine.SkillSystem
{
    public class WarriorMinionComponent : WarriorAIComponent
    {
        private readonly MinionAIComponent _aiComponent;
            
        public WarriorMinionComponent(IHumanoid Parent, IHumanoid Owner) : base(Parent, default(bool))
        {
            IgnoreEntities = new IEntity[] { Owner };
            _aiComponent = new MinionAIComponent(Parent, Owner);
            _aiComponent.AlterBehaviour<AttackBehaviour>(
                new WarriorMinionBehaviour(Parent, (T, M) =>
                {
                    Parent.RotateTowards(T);
                    base.OnAttack();
                })
            );
        }
            
        public override void Update()
        {
            _aiComponent.Update();
            base.DoUpdate();
        }

        public override void Dispose()
        {
            base.Dispose();
            _aiComponent.Dispose();
        }
        
        private class WarriorMinionBehaviour : AttackBehaviour
        {
            private readonly Action<IEntity, float> _lambda;
            public WarriorMinionBehaviour(IEntity Parent, Action<IEntity, float> Lambda) : base(Parent)
            {
                _lambda = Lambda;
            }

            protected override void Attack(float RangeModifier)
            {
                _lambda(Target, RangeModifier);
            }
        }
    }
}