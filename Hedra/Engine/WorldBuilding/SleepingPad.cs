using System;
using System.Windows.Forms;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Events;
using Hedra.Engine.Game;
using Hedra.Engine.Player;
using Hedra.Engine.Scenes;
using Hedra.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Game;
using Hedra.Localization;
using OpenTK;
using OpenTK.Input;
using KeyEventArgs = Hedra.Engine.Events.KeyEventArgs;

namespace Hedra.Engine.WorldBuilding
{
    public class SleepingPad : InteractableStructure, IUpdatable, ITickable
    {
        public bool IsOccupied => Sleeper != null;
        public IHumanoid Sleeper { get; private set; }
        public Vector3 TargetRotation { get; set; }
        
        protected override bool DisposeAfterUse => false;

        protected override bool CanInteract => SkyManager.IsSleepTime && !IsOccupied;

        public SleepingPad(Vector3 Position) : base(Position)
        {
        }

        public override string Message => Translations.Get("to_sleep", Controls.Interact);

        public override int InteractDistance => 12;

        public override void Update()
        {
            base.Update();
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
                Sleeper.Physics.TargetPosition += Vector3.UnitY;
                var dmgComponent = Sleeper.SearchComponent<DamageComponent>();
                if (dmgComponent != null)
                {
                    dmgComponent.OnDamageEvent -= this.OnDamageWakeUp;
                }
            }
            if (Human != null)
            {
                Human.ShowIcon(CacheItem.SleepingIcon);
                Human.IsSleeping = true;
                Human.IsRiding = false;
                Human.CanInteract = false;
                Human.Physics.TargetPosition = Position;
                Human.Rotation = TargetRotation;
                var dmgComponent = Human.SearchComponent<DamageComponent>();
                if (dmgComponent != null)
                {
                    dmgComponent.Immune = false;
                    dmgComponent.OnDamageEvent += this.OnDamageWakeUp;
                }
            }
            Sleeper = Human;
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
