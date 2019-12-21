using System;
using System.Linq;
using Hedra.Components;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.AISystem.Humanoid
{
    public abstract class CombatAIComponent : TraverseHumanoidAIComponent
    {
        public const float StareRadius = 16;
        private Vector3 _targetPoint;
        private bool _hasTargetPoint;
        private bool _staring;
        private bool _isFriendly;
        private IEntity _chasingTarget;
        private CombatAIBehaviour _behaviour;
        private readonly Vector3 _originalPosition;
        private readonly Timer _movementTimer;
        private readonly Timer _rollTimer;
        private readonly Timer _forgetTimer;
        private bool _guardSpawnPoint;
        private bool _canExplore;
        protected abstract float SearchRadius { get; }
        protected abstract float AttackRadius { get; }
        protected abstract float ForgetRadius { get; }
        public IEntity[] IgnoreEntities { get; set; } = new IEntity[0];
        protected override bool ShouldSleep => !IsChasing;
        public bool IsChasing => _chasingTarget != null;
        public bool IsExploring => !IsChasing && _hasTargetPoint;
        protected virtual bool CanExplore => _canExplore;
        protected virtual bool GuardSpawnPoint => _guardSpawnPoint;
        public virtual bool DontUpdateAI => false;
        public bool CanForgetTargets { get; set; } = true;

        protected CombatAIComponent(IHumanoid Entity, bool IsFriendly) : base(Entity)
        {
            _canExplore = true;
            _guardSpawnPoint = true;
            _isFriendly = IsFriendly;
            _movementTimer = new Timer(1);
            _rollTimer = new Timer(Utils.Rng.NextFloat() * 3 + 4.0f);
            _forgetTimer = new Timer(Utils.Rng.NextFloat() * 6 + 8.0f);
            Behaviour = new BanditAIBehaviour(Entity);
            _targetPoint = Behaviour.FindPoint();
            _originalPosition = Parent.Position;
            _movementTimer.MarkReady();
        }

        protected override bool ShouldWakeup
        {
            get
            {
                var humanoids = World.InRadius<Engine.Player.Humanoid>(this.Parent.Position, 64f);
                if (humanoids == null) return false;
                for (var i = 0; i < humanoids.Length; i++)
                {
                    var combatAI = humanoids[i].SearchComponent<CombatAIComponent>();
                    if (combatAI != null)
                    {
                        if (humanoids[i].IsAttacking)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        protected override void OnDamageEvent(DamageEventArgs Args)
        {
            base.OnDamageEvent(Args);
            if(!Array.Exists(IgnoreEntities, E => E == Args.Damager))
                SetTarget(Args.Damager);
        }

        public override void Update()
        {
            base.Update();
            if(DontUpdateAI) return;
            if (!CanUpdate) return;
            if (ShouldReset()) return;
            DoUpdate();
            if (IsChasing) OnChasing();
            else if(!_staring && CanExplore) OnExploring();
            if (IsExploring) HandleStaring();
            else _staring = false;
            FindTarget();
        }

        private void HandleStaring()
        {
            var nearHumanoid = World.InRadius<IPlayer>(Parent.Position, StareRadius).FirstOrDefault(H => !H.IsInvisible);
            if (nearHumanoid != null)
            {
                Stare(nearHumanoid);
            }
        }

        private void Stare(IEntity Entity)
        {
            Parent.LookAt(Entity);
            if (_hasTargetPoint)
            {
                CancelMovement();
                _hasTargetPoint = false;
                Behaviour.OnStare(Entity);
            }
            _staring = true;
        }

        public void WalkTo(Vector3 Position)
        {
            CancelMovement();
            SetTargetPoint(Position);
            MoveTo(_targetPoint);
        }

        private bool ShouldReset()
        {
            var targetLost = !IsChasing && GuardSpawnPoint && (_targetPoint.Xz() - Parent.Position.Xz()).LengthSquared() > ForgetRadius * ForgetRadius;
            var targetDead = IsChasing && (_chasingTarget.IsDead || _chasingTarget.IsInvisible);
            var shouldWeReset = IsChasing && _forgetTimer.Tick() && CanForgetTargets || targetLost && false || targetDead;
            if (shouldWeReset)
            {
                Reset();
                return true;
            }
            return false;
        }
        
        protected virtual void DoUpdate()
        {
            
        }

        private void OnChasing()
        {
            SetTargetPoint(_chasingTarget.Position);
            if (InAttackRadius(_chasingTarget) && !Parent.IsKnocked)
            {
                Orientate(_targetPoint);
                OnAttack();
                _forgetTimer.Reset();
                CancelMovement();
            }
            else
            {
                MoveTo(_targetPoint);
            }
            Behaviour.SetTarget(_chasingTarget);
        }
        
        private void OnExploring()
        {
            if (!_hasTargetPoint && _movementTimer.Tick())
            {
                /* When exploring without a target set the speed to 1 */
                Parent.AddBonusSpeedWhile(-Parent.Speed + .65f, () => IsExploring, false);
                SetTargetPoint(Behaviour.FindPoint());
                MoveTo(_targetPoint);
            }
        }

        protected override void OnTargetPointReached()
        {
            _hasTargetPoint = false;
        }

        protected override void OnMovementStuck()
        {
            _hasTargetPoint = false;
            Behaviour.OnStuck();
        }

        protected abstract void OnAttack();

        protected virtual bool InAttackRadius(IEntity Target)
        {
            return (Target.Position - Parent.Position).LengthSquared() < AttackRadius * AttackRadius;
        }
        
        private void Reset()
        {
            if (IsChasing && _chasingTarget.IsDead)
                Parent.Health += _chasingTarget.MaxHealth * .33f;

            _chasingTarget = null;
            SetTargetPoint(_originalPosition);
            MoveTo(_targetPoint);
        }

        protected void RollAndMove2()
        {
            if(Parent != null && Parent.WasAttacking) return;
            if (_rollTimer.Tick() && Parent != null && (_targetPoint.Xz() - Parent.Position.Xz()).LengthSquared() > AttackRadius * AttackRadius)
                Parent.Roll(RollType.Normal);
        }

        private void FindTarget()
        {
            if (IsChasing) return;
            SetTarget(_isFriendly 
                ? Behaviour.FindMobTarget(48) 
                : Behaviour.FindPlayerTarget(SearchRadius)
            );
        }

        public void SetTarget(IEntity Target)
        {
            _chasingTarget = Target;
            _forgetTimer.Reset();
        }

        private void SetTargetPoint(Vector3 Position)
        {
            _hasTargetPoint = true;
            _targetPoint = Position;
        }

        public void SetCanExplore(bool Value) => _canExplore = Value;

        public void SetGuardSpawnPoint(bool Value) => _guardSpawnPoint = Value;
        
        public IEntity ChasingTarget => _chasingTarget;
        
        public CombatAIBehaviour Behaviour
        {
            get => _behaviour;
            set
            {
                _behaviour = value;
                _movementTimer.AlertTime = _behaviour.WaitTime;
            }
        }
    }
}
