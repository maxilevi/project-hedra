using System;
using System.Numerics;
using Hedra.AISystem.Behaviours;
using Hedra.Components;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.EntitySystem;

namespace Hedra.AISystem
{
    public class MinionAIComponent : BasicAIComponent
    {
        private const int MaxSeparatedDistance = 128 * 128;
        private readonly AttackBehaviour _attack;
        private readonly FollowBehaviour _follow;
        private bool _disposed;

        public MinionAIComponent(IEntity Parent, IEntity Owner) : base(Parent)
        {
            this.Owner = Owner;
            this.Owner.SearchComponent<DamageComponent>().OnDamageEvent += OnDamage;
            this.Owner.AfterDamaging += OnDamaging;
            this.Owner.Kill += OnOwnerKill;
            Parent.SearchComponent<DamageComponent>().OnDamageEvent += OnDamage;
            Parent.BeforeDamaging += BeforeDamaging;
            Parent.AfterDamaging += AfterDamaging;
            Parent.Kill += OnKill;
            _attack = new AttackBehaviour(Parent);
            _follow = new FollowBehaviour(Parent)
            {
                Target = this.Owner,
                ErrorMargin = 4 * Chunk.BlockSize
            };
        }

        public IEntity Owner { get; }
        public bool Enabled { get; set; } = true;

        public override AIType Type => throw new NotImplementedException();

        public override void Update()
        {
            if (!Enabled) return;
            if ((Parent.Position - Owner.Position).LengthSquared() > MaxSeparatedDistance)
            {
                Parent.Position = Owner.Position + Vector3.Cross(Parent.Orientation, Vector3.UnitY) * 12f;
                Parent.Physics.ResetFall();
                _attack.ResetTarget();
                _follow.Cancel();
                _follow.Target = Owner;
            }

            if (_attack.Enabled)
                _attack.Update();
            else
                _follow.Update();
        }

        private void OnDamage(DamageEventArgs Args)
        {
            if (Args.Damager != Owner && Args.Damager != Parent && Args.Damager != null && _attack.Target == null)
                _attack.SetTarget(Args.Damager);
        }

        private void OnDamaging(IEntity Target, float Damage)
        {
            if (Target != Owner && Target != Parent && Target != null && _attack.Target == null)
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
            if (Owner is IHumanoid humanoid)
                humanoid.XP += Args.Experience;
        }

        private void OnOwnerKill(DeadEventArgs Args)
        {
            if (Parent.SearchComponent<CompanionStatsComponent>() != null)
                Parent.SearchComponent<CompanionStatsComponent>().XP += Args.Experience;
        }

        private void Kill()
        {
            if (_disposed) return;
            _disposed = true;
            Parent.Health = 0;
            Executer.ExecuteOnMainThread(Dispose);
        }

        public override void Dispose()
        {
            base.Dispose();
            _follow.Dispose();
            _attack.Dispose();
            Owner.SearchComponent<DamageComponent>().OnDamageEvent -= OnDamage;
            Owner.AfterDamaging -= OnDamaging;
            Owner.Kill -= OnOwnerKill;
        }
    }
}