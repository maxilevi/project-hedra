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
    public abstract class CombatAIComponent : HumanoidAIComponent
    {
        public bool Friendly { get; set; }
        protected Vector3 TargetPoint;
        protected bool Chasing;
        protected Entity ChasingTarget;
        protected Vector3 OriginalPosition;
        protected Timer MovementTimer;
        protected Timer RollTimer;
        public abstract float SearchRadius { get; set; }
        public abstract float AttackRadius { get; set; }
        public abstract float ForgetRadius { get; set; }

        protected CombatAIComponent(Entity Entity, bool Friendly) : base(Entity)
        {
            this.Friendly = Friendly;
            this.TargetPoint = new Vector3(Utils.Rng.NextFloat() * 24 - 12f, 0, Utils.Rng.NextFloat() * 24 - 12f) + Parent.BlockPosition;
            this.MovementTimer = new Timer(6.0f);
            this.RollTimer = new Timer(4.0f);
            this.OriginalPosition = Parent.BlockPosition;
        }

        public override void Update()
        {
            if (Parent.Knocked) return;

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

        protected void Roll()
        {
            var human = Parent as Humanoid;
            if(human != null && human.WasAttacking) return;
            if (RollTimer.Tick() && human != null && (TargetPoint.Xz - Parent.Position.Xz).LengthSquared > AttackRadius * AttackRadius)
                human.Roll();

            Parent.Model.Run();
            Parent.Physics.Move(Parent.Orientation * Parent.Speed * 5 * 2);
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

                            this.Chasing = true;
                            this.ChasingTarget = World.Entities[i];
                            break;
                        }
                    }
                }
                else
                {
                    Humanoid player = Scenes.SceneManager.Game.LPlayer;
                    if ((player.Position.Xz - Parent.Position.Xz).LengthSquared < SearchRadius * SearchRadius)
                    {
                        this.Chasing = true;
                        this.ChasingTarget = player;
                    }
                }
            }
        }
    }
}
