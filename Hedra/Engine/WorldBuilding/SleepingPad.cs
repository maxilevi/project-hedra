using System;
using System.Windows.Forms;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Events;
using Hedra.Engine.Player;
using Hedra.Engine.Scenes;
using Hedra.EntitySystem;
using Hedra.Engine.Localization;
using OpenTK;
using OpenTK.Input;
using KeyEventArgs = Hedra.Engine.Events.KeyEventArgs;

namespace Hedra.Engine.WorldBuilding
{
    public class SleepingPad : BaseStructure, IUpdatable, ITickable
    {
        public bool IsOccupied => Sleeper != null;
        public IHumanoid Sleeper { get; private set; }
        public int BedRadius { get; set; } = 12;
        public Vector3 TargetRotation { get; set; }

        public SleepingPad(Vector3 Position) : base(Position)
        {
            var player = LocalPlayer.Instance;
            EventDispatcher.RegisterKeyDown(this, delegate(object Sender, KeyEventArgs Args)
            {
                if (Args.Key == Controls.Interact && !IsOccupied)
                {
                    if (player.IsAttacking || player.IsCasting || player.IsDead || !player.CanInteract ||
                        player.IsEating || (player.Position - this.Position).LengthSquared > BedRadius * BedRadius || !SkyManager.IsNight) return;

                    this.SetSleeper(player);
                }
                if (Args.Key == Controls.Descend && IsOccupied && Sleeper == player)
                {
                    this.SetSleeper(null);
                }
            });
            
            UpdateManager.Add(this);      
        }

        public void Update()
        {
            var player = LocalPlayer.Instance;
            player.MessageDispatcher.ShowMessageWhile(Translations.Get("to_sleep", Controls.Interact),
                () => (player.Position - this.Position).LengthSquared < BedRadius * BedRadius && player.CanInteract && !IsOccupied && SkyManager.IsNight);

            if(IsOccupied && (!SkyManager.IsNight || Sleeper.IsDead))
                this.SetSleeper(null);
        }

        public void SetSleeper(IHumanoid Human)
        {
            if (Sleeper != null)
            {
                Sleeper.IsSleeping = false;
                Sleeper.CanInteract = true;
                Sleeper.ShowIcon(null);
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

        public void OnDamageWakeUp (DamageEventArgs Args)
        {
            if (Sleeper == Args.Victim)
                this.SetSleeper(null);
        }

        public override void Dispose()
        {
            EventDispatcher.UnregisterKeyDown(this);
            UpdateManager.Remove(this);
        }
    }
}
