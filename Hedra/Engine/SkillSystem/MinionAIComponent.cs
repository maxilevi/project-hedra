using System;
using Hedra.AISystem;
using Hedra.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.SkillSystem.Archer.Hunter;
using Hedra.EntitySystem;

namespace Hedra.Engine.SkillSystem
{ 
    public class MinionAIComponent : BasicAIComponent
    {
        private readonly FollowBehaviour _follow;
        private readonly AttackBehaviour _attack;
        private readonly IEntity _owner;
        private bool _disposed;

        public MinionAIComponent(IEntity Parent, IEntity Owner) : base(Parent)
        {
            _owner = Owner;
            _owner.SearchComponent<DamageComponent>().OnDamageEvent += OnDamage;
            _owner.AfterDamaging += OnDamaging;
            _attack = new AttackBehaviour(Parent);
            _follow = new FollowBehaviour(Parent)
            {
                Target = _owner,
                ErrorMargin = 16
            };
        }

        public override void Update()
        {
            if (_attack.Enabled)
            {
                _attack.Update();
            }
            else
            {
                _follow.Update();
            }

            if (_owner.IsDead) Kill();
        }

        private void OnDamage(DamageEventArgs Args)
        {
            if(Args.Damager != _owner && Args.Damager != Parent && Args.Damager != null)
                _attack.SetTarget(Args.Damager);
        }

        private void OnDamaging(IEntity Target, float Damage)
        {
            if (Target != _owner && Target != Parent && Target != null)
                _attack.SetTarget(Target);
        }

        private void Kill()
        {
            if(_disposed) return;
            _disposed = true;
            Executer.ExecuteOnMainThread(Dispose);
        }
        
        public override void Dispose()
        {
            base.Dispose();
            _owner.SearchComponent<DamageComponent>().OnDamageEvent -= OnDamage;
            _owner.AfterDamaging -= OnDamaging;
        }

        public override AIType Type => throw new NotImplementedException();
    }
}