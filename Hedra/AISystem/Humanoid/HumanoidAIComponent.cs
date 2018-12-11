using System;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.AISystem.Humanoid
{
    public abstract class HumanoidAIComponent : Component<IHumanoid>
    {
        private SleepingPad _bed;
        private bool _isDrowning;
        protected bool IsMoving { get; private set; }
        protected bool IsSleeping { get; private set; }
        protected abstract bool ShouldSleep { get; }
        protected virtual bool ShouldWakeup { get; }
        protected bool CanUpdate => !IsSleeping && !_isDrowning && !Parent.IsKnocked;

        protected HumanoidAIComponent(IHumanoid Entity) : base(Entity)
        {
            var dmgComponent = Parent.SearchComponent<DamageComponent>();
            if (dmgComponent != null)
            {
                dmgComponent.OnDamageEvent += OnDamageEvent;
            }
        }

        protected void Orientate(Vector3 TargetPoint)
        {
            Parent.Orientation = (TargetPoint - Parent.Position).Xz.NormalizedFast().ToVector3();
            Parent.Model.TargetRotation = Physics.DirectionToEuler(Parent.Orientation);
        }

        protected virtual void OnDamageEvent(DamageEventArgs Args)
        {
            if (IsSleeping)
            {
                this.Wakeup();
                this.Parent.KnockForSeconds(3.0f);           
            }
        }

        public override void Update()
        {
            this.ManageDrowning();
            this.ManageSleeping();
        }

        private void ManageDrowning()
        {
            if (Parent.Oxygen <= 0) _isDrowning = true;
            if (_isDrowning)
            {
                if(Parent.IsUnderwater) Parent.Movement.MoveInWater(true);
                if (Parent.Oxygen >= Parent.MaxOxygen * .75f) _isDrowning = false;
            }
        }
        
        private void ManageSleeping()
        {
            this.ManageSleepingState();
            if (ShouldSleep && !IsSleeping && (SkyManager.DayTime > 19000 || SkyManager.DayTime < 8000))
            {
                var nearestBeds = World.InRadius<SleepingPad>(Parent.Position, 32f);
                if(nearestBeds == null) return;
                for (var i = 0; i < nearestBeds.Length; i++)
                {
                    if (!nearestBeds[i].IsOccupied)
                    {
                        nearestBeds[i].SetSleeper(Parent as Engine.Player.Humanoid);
                        _bed = nearestBeds[i];
                        IsSleeping = true;
                        break;
                    }
                }
            }else if (IsSleeping && (SkyManager.DayTime < 19000 && SkyManager.DayTime > 8000 || ShouldWakeup))
            {
                Wakeup();
            }
        }

        private void ManageSleepingState()
        {
            if (_bed != null)
            {
                if (_bed.Sleeper == null)
                {
                    _bed = null;
                    this.IsSleeping = false;
                }
            }
            if (this.IsSleeping)
            {
                if (_bed == null)
                    this.IsSleeping = false;
            }
        }

        private void Wakeup()
        {
            IsSleeping = false;
            _bed.SetSleeper(null);
            _bed = null;
        }

        /// <summary>
        /// Move to target position. Needs to be called every frame.
        /// </summary>
        /// <param name="TargetPoint">Target point to move</param>
        protected void Move(Vector3 TargetPoint)
        {
            if ((TargetPoint.Xz - Parent.Position.Xz).LengthSquared > 3 * 3)
            {
                Parent.Orientation = (TargetPoint.Xz - Parent.Position.Xz).ToVector3().NormalizedFast();
                Parent.Model.TargetRotation = Physics.DirectionToEuler(Parent.Orientation);
                Parent.Physics.Move();
                Parent.IsSitting = false;
                IsMoving = true;
            }
            else
            {
                IsMoving = false;
            }
            if (Parent.IsUnderwater)
            {
                if (Math.Abs(TargetPoint.Y - Parent.Position.Y) > 1)
                    Parent.Movement.MoveInWater(TargetPoint.Y > Parent.Position.Y);
            }
        }

        protected void Sit()
        {
            Parent.IsSitting = true;
        }
    }
}
