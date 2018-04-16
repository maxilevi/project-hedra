using System;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Events;
using Hedra.Engine.Player;
using Hedra.Engine.Scenes;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.QuestSystem
{
    public class SleepingPad : BaseStructure, IUpdatable, ITickable
    {
        public bool IsOccupied => Sleeper != null;
        public Humanoid Sleeper { get; private set; }
        public int BedRadius { get; set; } = 16;
        public Vector3 TargetRotation { get; set; }

        public SleepingPad(Vector3 Position)
        {
            this.Position = Position;
            var player = LocalPlayer.Instance;
            EventDispatcher.RegisterKeyDown(this, delegate(object sender, KeyboardKeyEventArgs Args)
            {
                if (Args.Key == Key.E && !IsOccupied)
                {
                    if (player.IsAttacking || player.IsCasting || player.IsDead || !player.CanInteract ||
                        player.IsEating || (player.Position - this.Position).LengthSquared > BedRadius * BedRadius || !SkyManager.IsNight) return;

                    this.SetSleeper(player);
                }
                if (Args.Key == Key.ShiftLeft && IsOccupied && Sleeper == player)
                {
                    this.SetSleeper(null);
                }
            });
            
            UpdateManager.Add(this);      
        }

        public void Update()
        {
            var player = LocalPlayer.Instance;
            player.MessageDispatcher.ShowMessageWhile("[E] TO SLEEP",
                () => (player.Position - this.Position).LengthSquared < BedRadius * BedRadius && player.CanInteract && !IsOccupied && SkyManager.IsNight);

            if(IsOccupied && !SkyManager.IsNight)
                this.SetSleeper(null);
            if(IsOccupied && Sleeper.IsDead)
                this.SetSleeper(null);
        }

        public void SetSleeper(Humanoid Human)
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
        }
    }
}
