using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.AISystem.Humanoid
{
    public abstract class CombatAIComponent : TraverseHumanoidAIComponent
    {
        public bool Friendly { get; set; }
        protected Vector3 TargetPoint;
        protected bool Chasing;
        protected IEntity ChasingTarget;
        protected Vector3 OriginalPosition;
        protected Timer MovementTimer;
        protected Timer RollTimer;
        public bool CanDodge { get; set; } = true;
        public abstract float SearchRadius { get; set; }
        public abstract float AttackRadius { get; set; }
        public abstract float ForgetRadius { get; set; }
        protected override bool ShouldSleep => !Chasing;

        protected CombatAIComponent(IHumanoid Entity, bool Friendly) : base(Entity)
        {
            this.Friendly = Friendly;
            this.TargetPoint = new Vector3(Utils.Rng.NextFloat() * 24 - 12f, 0, Utils.Rng.NextFloat() * 24 - 12f) + Parent.BlockPosition;
            this.MovementTimer = new Timer(Utils.Rng.NextFloat() * 4 + 6.0f);
            this.RollTimer = new Timer(Utils.Rng.NextFloat() * 3 + 4.0f);
            this.OriginalPosition = Parent.BlockPosition;
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
            if (!(Args.Damager is LocalPlayer)) return;
            this.SetTarget(Args.Damager);
        }

        public override void Update()
        {
            base.Update();
            if (!base.CanUpdate) return;
            this.DoUpdate();
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

        protected void RollAndMove2()
        {
            if(Parent != null && Parent.WasAttacking) return;
            if (RollTimer.Tick() && Parent != null && (TargetPoint.Xz - Parent.Position.Xz).LengthSquared > AttackRadius * AttackRadius && CanDodge)
                Parent.Roll(RollType.Normal);
        }

        protected void LookTarget()
        {
            base.Orientate(TargetPoint);
            if (!Chasing)
            {
                if (Friendly)
                {
                    for (var i = World.Entities.Count - 1; i > -1; i--)
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
                    var player = GameManager.Player;
                    if ((player.Position.Xz - Parent.Position.Xz).LengthSquared < SearchRadius * SearchRadius)
                    {
                        this.SetTarget(player);
                    }
                }
            }
        }

        protected virtual void SetTarget(IEntity Target)
        {
            this.Chasing = true;
            this.ChasingTarget = Target;
        }
    }
}
