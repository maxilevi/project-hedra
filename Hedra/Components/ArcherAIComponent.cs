/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 08/12/2016
 * Time: 11:13 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.WeaponSystem;
using OpenTK;

namespace Hedra.Components
{
    /// <summary>
    /// Description of ArcherAI.
    /// </summary>
    public class ArcherAIComponent : CombatAIComponent
    {
        private float _secondAttackCooldown;
        private float _firstAttackCooldown;
        public override float SearchRadius { get; set; } = 128;
        public override float AttackRadius { get; set; } = 96;
        public override float ForgetRadius { get; set; } = 192;

        public ArcherAIComponent(IHumanoid Parent, bool Friendly) : base(Parent, Friendly){}
        
        public override void DoUpdate()
        {        
            _secondAttackCooldown -= Time.IndependantDeltaTime;
            _firstAttackCooldown -= Time.IndependantDeltaTime;

            if( this.MovementTimer.Tick() && !Chasing)
                this.TargetPoint = new Vector3(Utils.Rng.NextFloat() * 24-12f, 0, Utils.Rng.NextFloat() * 24-12f) + Parent.BlockPosition;
            
            else if(Chasing)
            {
                
                if((TargetPoint.Xz - Parent.Position.Xz).LengthSquared > ForgetRadius * ForgetRadius || ChasingTarget.IsDead || ChasingTarget.IsInvisible)
                {
                    base.Reset();
                    return;
                }
                
                this.TargetPoint = ChasingTarget.Position;
                if( (TargetPoint - Parent.Position).LengthSquared < AttackRadius * AttackRadius && !Parent.IsKnocked)
                {
                    if (Parent is Humanoid human)
                    {
                        if (human.LeftWeapon is Bow bow)
                        {
                            bow.ArrowDownForce = 1-(TargetPoint - Parent.Position).LengthFast / (AttackRadius+16);
                            if (_secondAttackCooldown <= 0)
                            {
                                _secondAttackCooldown = 4.5f;
                                bow.Attack2(human);
                            }
                            else if (_firstAttackCooldown <= 0)
                            {
                                _firstAttackCooldown = 1.5f;
                                bow.Attack1(human);
                            }
                        }
                    }
                }else
                {

                    base.RollAndMove();
                }
            }
            base.LookTarget();
        }
    }
}
