using System;
using System.Linq;
using Hedra.Components;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.AISystem.Humanoid
{
    public abstract class BaseHumanoidAIComponent : Component<IHumanoid>, IBehaviourComponent
    {
        protected const float DefaultErrorMargin = 3;
        private SleepingPad _bed;
        private bool _isDrowning;
        protected bool IsMoving { get; set; }
        protected bool IsSleeping { get; private set; }
        protected abstract bool ShouldSleep { get; }
        protected virtual bool ShouldWakeup { get; }
        protected virtual bool UseLantern => true;
        private readonly Timer _lanternTimer;

        protected bool CanUpdate => !IsSleeping && !_isDrowning && !Parent.IsKnocked;

        protected BaseHumanoidAIComponent(IHumanoid Entity) : base(Entity)
        {
            var dmgComponent = Parent.SearchComponent<DamageComponent>();
            if (dmgComponent != null)
            {
                dmgComponent.OnDamageEvent += OnDamageEvent;
            }
            _lanternTimer = new Timer(Utils.Rng.NextFloat() + .005f);
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
            this.ManageLantern();
            this.ManageDrowning();
            this.ManageSleeping();
        }

        private void ManageLantern()
        {
            if(!UseLantern || !_lanternTimer.Tick()) return;
            var lights = ShaderManager.Lights;
            var insideAnyLight = lights.Any(L => L != Parent.HandLamp.LightObject && L.Collides(Parent.Position));
            var newValue = false;
            if (!insideAnyLight) newValue = SkyManager.IsNight;
            if(Parent.HandLamp.Enabled != newValue)
                Parent.HandLamp.Enabled = newValue;
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

        protected virtual void OnMovementStuck()
        {
        }

        protected virtual void OnTargetPointReached()
        {
            
        }

        protected void Sit()
        {
            Parent.IsSitting = true;
        }
    }
}