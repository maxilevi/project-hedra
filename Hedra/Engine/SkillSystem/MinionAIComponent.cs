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
        public IHumanoid Owner { get; }
        private readonly FollowBehaviour _follow;
        private readonly AttackBehaviour _attack;
        private bool _disposed;

        public MinionAIComponent(IEntity Parent, IHumanoid Owner) : base(Parent)
        {
            this.Owner = Owner;
            this.Owner.SearchComponent<DamageComponent>().OnDamageEvent += OnDamage;
            this.Owner.AfterDamaging += OnDamaging;
            Parent.BeforeDamaging += BeforeDamaging;
            Parent.AfterDamaging += AfterDamaging;
            Parent.Kill += OnKill;
            _attack = new AttackBehaviour(Parent);
            _follow = new FollowBehaviour(Parent)
            {
                Target = this.Owner,
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

            if (Owner.IsDead) Kill();
        }

        private void OnDamage(DamageEventArgs Args)
        {
            if(Args.Damager != Owner && Args.Damager != Parent && Args.Damager != null)
                _attack.SetTarget(Args.Damager);
        }

        private void OnDamaging(IEntity Target, float Damage)
        {
            if (Target != Owner && Target != Parent && Target != null)
                _attack.SetTarget(Target);
        }

        private void BeforeDamaging(IEntity Target, float Damage)
        {
            Owner.InvokeBeforeDamaging(Target, Damage);
        }
        
        private void AfterDamaging(IEntity Target, float Damage)
        {
            Owner.InvokeAfterDamaging(Target, Damage);
        }

        private void OnKill(DeadEventArgs Args)
        {
            Owner.XP += Args.Experience;
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
            Owner.SearchComponent<DamageComponent>().OnDamageEvent -= OnDamage;
            Owner.AfterDamaging -= OnDamaging;
        }

        public override AIType Type => throw new NotImplementedException();
    }
}