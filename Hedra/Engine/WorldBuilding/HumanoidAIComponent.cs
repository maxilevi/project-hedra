using Hedra.Engine.EntitySystem;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using OpenTK;

namespace Hedra.Engine.WorldBuilding
{
    public abstract class HumanoidAIComponent : EntityComponent
    {
        private SleepingPad _bed;
        protected bool IsSleeping { get; private set; }
        public abstract bool ShouldSleep { get; }
        public virtual bool ShouldWakeup { get; }

        protected HumanoidAIComponent(Entity Entity) : base(Entity)
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

        protected void ManageSleeping()
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
                        nearestBeds[i].SetSleeper(Parent as Humanoid);
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
                if (Parent is Humanoid human)
                {
                    human.Physics.DeltaTranslate(human.Movement.MoveFormula(Parent.Orientation) * .75f);
                }
                else
                {
                    Parent.Physics.DeltaTranslate(Parent.Orientation * Parent.Speed * 4);
                }
                Parent.Model.Run();
            }
            else
            {
                Parent.Model.Idle();
            }
        }

        protected void Sit()
        {
            if (Parent is Humanoid human)
            {
                human.Model.Sit();
            }
            else
            {
                Parent.Model.Idle();
            }
        }
    }
}
