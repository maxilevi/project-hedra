 /*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 08/12/2016
 * Time: 11:13 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.Management;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.AISystem.Humanoid
{
    /// <summary>
    /// Description of WarriorAI.
    /// </summary>
    public class WarriorAIComponent : CombatAIComponent
    {
        private float _attackTimer;
        private Timer ForgetTimer { get; set; }
        public override float SearchRadius { get; set; } = 64;
        public override float AttackRadius { get; set; } = 0;
        public override float ForgetRadius { get; set; } = 64;

        public WarriorAIComponent(IHumanoid Parent, bool Friendly) : base(Parent, Friendly)
        {
            this._attackTimer = 0f;
            this.ForgetTimer = new Timer(8f);

        }

        protected override void SetTarget(IEntity Target)
        {
            base.SetTarget(Target);
            ForgetTimer.Reset();
        }

        public override void DoUpdate()
        {
            if (this.MovementTimer.Tick() && !Chasing)
            {
                base.MoveTo(TargetPoint = new Vector3(Utils.Rng.NextFloat() * 24 - 12f, 0, Utils.Rng.NextFloat() * 24 - 12f) +
                                   Parent.BlockPosition);
            }
            else if (Chasing)
            {

                if (ForgetTimer.Tick() ||
                    ChasingTarget.IsDead || ChasingTarget.IsInvisible)
                {
                    base.Reset();
                    return;
                }

                this.TargetPoint = ChasingTarget.Position;
                this._attackTimer -= Time.IndependantDeltaTime;
                if (Parent.InAttackRange(ChasingTarget, 1.5f) && !Parent.IsKnocked)
                {
                    base.Orientate(TargetPoint);
                    if (_attackTimer < 0)
                    {
                        
                        Parent.LeftWeapon.Attack1(Parent);
                        _attackTimer = 1.25f;
                    }
                    this.ForgetTimer.Reset();
                }
                else
                {
                    base.MoveTo(TargetPoint);
                }
            }

            base.LookTarget();
        }
    }
}
