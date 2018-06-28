using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using OpenTK;

namespace Hedra.Engine.QuestSystem
{
    internal abstract class CombatAIComponent : HumanoidAIComponent
    {
        public bool Friendly { get; set; }
        protected Vector3 TargetPoint;
        protected bool Chasing;
        protected Entity ChasingTarget;
        protected Vector3 OriginalPosition;
        protected Timer MovementTimer;
        protected Timer RollTimer;
        public bool CanDodge { get; set; } = true;
        public abstract float SearchRadius { get; set; }
        public abstract float AttackRadius { get; set; }
        public abstract float ForgetRadius { get; set; }
        public override bool ShouldSleep => !Chasing;

        protected CombatAIComponent(Entity Entity, bool Friendly) : base(Entity)
        {
            this.Friendly = Friendly;
            this.TargetPoint = new Vector3(Utils.Rng.NextFloat() * 24 - 12f, 0, Utils.Rng.NextFloat() * 24 - 12f) + Parent.BlockPosition;
            this.MovementTimer = new Timer(6.0f);
            this.RollTimer = new Timer(4.0f);
            this.OriginalPosition = Parent.BlockPosition;
        }

        public override bool ShouldWakeup
        {
            get
            {
                var humanoids = World.InRadius<Humanoid>(this.Parent.Position, 64f);
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
            if (Args.Damager == null) return;
            this.SetTarget(Args.Damager);
        }

        public override void Update()
        {
            if (Parent.Knocked) return;

            if(!this.IsSleeping)
                this.DoUpdate();
            this.ManageSleeping();
        }

        public abstract void DoUpdate();

        protected void Reset()
        {
            if (ChasingTarget.IsDead)
                Parent.Health += ChasingTarget.MaxHealth * .33f;

            ChasingTarget = null;
            Chasing = false;
            this.TargetPoint = this.OriginalPosition;
        }

        protected void Roll()
        {
            var human = Parent as Humanoid;
            if(human != null && human.WasAttacking) return;
            if (RollTimer.Tick() && human != null && (TargetPoint.Xz - Parent.Position.Xz).LengthSquared > AttackRadius * AttackRadius && CanDodge)
                human.Roll();

            Parent.Model.Run();
            Parent.Physics.Move(human?.Movement.MoveFormula(Parent.Orientation) * (float) Time.deltaTime ?? Vector3.Zero);
        }

        protected void LookTarget()
        {
            base.Orientate(TargetPoint);
            if (!Chasing)
            {
                this.Move(TargetPoint);

                if (Friendly)
                {
                    for (int i = World.Entities.Count - 1; i > -1; i--)
                    {
                        if (World.Entities[i] != this.Parent &&
                            (World.Entities[i].Position.Xz - Parent.Position.Xz).LengthSquared < 32 * 32)
                        {

                            if (World.Entities[i].IsStatic || World.Entities[i] is LocalPlayer
                                || World.Entities[i].IsImmune || World.Entities[i].IsFriendly ||
                                World.Entities[i].IsInvisible) continue;

                            this.SetTarget(World.Entities[i]);
                            break;
                        }
                    }
                }
                else
                {
                    Humanoid player = GameManager.Player;
                    if ((player.Position.Xz - Parent.Position.Xz).LengthSquared < SearchRadius * SearchRadius)
                    {
                        this.SetTarget(player);
                    }
                }
            }
        }

        protected virtual void SetTarget(Entity Target)
        {
            this.Chasing = true;
            this.ChasingTarget = Target;
        }
    }
}
