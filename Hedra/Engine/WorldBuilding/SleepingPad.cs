using Hedra.Components;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EnvironmentSystem;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Localization;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Core;
using KeyEventArgs = Hedra.Engine.Events.KeyEventArgs;

namespace Hedra.Engine.WorldBuilding
{
    public class SleepingPad : InteractableStructure
    {
        public bool IsOccupied => Sleeper != null;
        public IHumanoid Sleeper { get; private set; }
        public Vector3 TargetRotation { get; set; }
        protected override bool DisposeAfterUse => false;
        protected override bool SingleUse => false;
        protected override bool CanInteract => SkyManager.IsSleepTime && !IsOccupied;
        private readonly Counter _moveCooldown;
        
        public SleepingPad(Vector3 Position) : base(Position)
        {
            _moveCooldown = new Counter(4);
        }

        public override string Message => Translations.Get("to_sleep");
        public override int InteractDistance => 24;
        protected override bool AllowThroughCollider => true;

        public override void Update(float DeltaTime)
        {
            base.Update(DeltaTime);
            if(IsOccupied && (!SkyManager.IsSleepTime || Sleeper.IsDead))
                SetSleeper(null);
        }

        protected override void OnKeyDown(object Sender, KeyEventArgs Args)
        {
            base.OnKeyDown(Sender, Args);
            if (Args.Key == Controls.Descend && IsOccupied && Sleeper == GameManager.Player)
            {
                SetSleeper(null);
            }
        }

        public void SetSleeper(IHumanoid Human)
        {
            if (Sleeper != null)
            {
                Sleeper.IsSleeping = false;
                Sleeper.CanInteract = true;
                Sleeper.ShowIcon(null);
                Sleeper.Position += Vector3.UnitY;
                var dmgComponent = Sleeper.SearchComponent<DamageComponent>();
                if (dmgComponent != null)
                {
                    dmgComponent.OnDamageEvent -= this.OnDamageWakeUp;
                }
                Sleeper.Physics.OnMove -= OnMoveWakeUp;
            }
            if (Human != null)
            {
                Human.ShowIcon(CacheItem.SleepingIcon);
                Human.IsSleeping = true;
                Human.IsRiding = false;
                Human.CanInteract = false;
                Human.Position = Position;
                Human.Rotation = TargetRotation;
                var dmgComponent = Human.SearchComponent<DamageComponent>();
                if (dmgComponent != null)
                {
                    dmgComponent.Immune = false;
                    dmgComponent.OnDamageEvent += this.OnDamageWakeUp;
                }
                _moveCooldown.Reset();
                Human.Physics.OnMove += OnMoveWakeUp;
            }
            Sleeper = Human;
        }
        
        private void OnMoveWakeUp()
        {
            if(_moveCooldown.Tick())
                SetSleeper(null);
        }

        private void OnDamageWakeUp(DamageEventArgs Args)
        {
            if (Sleeper == Args.Victim)
                this.SetSleeper(null);
        }

        protected override void Interact(IHumanoid Humanoid)
        {
            SetSleeper(Humanoid);
        }
    }
}
