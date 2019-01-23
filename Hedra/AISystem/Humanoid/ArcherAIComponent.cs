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
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.WeaponSystem;
using OpenTK;

namespace Hedra.AISystem.Humanoid
{
    /// <summary>
    /// Description of ArcherAI.
    /// </summary>
    public class ArcherAIComponent : CombatAIComponent
    {
        private const int DefaultAttackRadius = 96;
        private float _secondAttackCooldown;
        private float _firstAttackCooldown;
        private Bow _leftWeapon;
        public override float SearchRadius { get; set; } = 128;
        public override float AttackRadius { get; set; } = DefaultAttackRadius;
        public override float ForgetRadius { get; set; } = 192;

        public ArcherAIComponent(IHumanoid Parent, bool Friendly) : base(Parent, Friendly)
        {
            _leftWeapon = (Bow) Parent.LeftWeapon;
            _leftWeapon.Miss += OnMiss;
            _leftWeapon.Hit += OnHit;
        }
        
        public override void DoUpdate()
        {        
            _secondAttackCooldown -= Time.IndependantDeltaTime;
            _firstAttackCooldown -= Time.IndependantDeltaTime;

            if( this.MovementTimer.Tick() && !Chasing)
                base.MoveTo(TargetPoint = new Vector3(Utils.Rng.NextFloat() * 24-12f, 0, Utils.Rng.NextFloat() * 24-12f) + Parent.BlockPosition);
            
            else if(Chasing)
            {
                
                if((TargetPoint.Xz - Parent.Position.Xz).LengthSquared > ForgetRadius * ForgetRadius || ChasingTarget.IsDead || ChasingTarget.IsInvisible)
                {
                    base.Reset();
                    return;
                }
                
                TargetPoint = ChasingTarget.Position;
                if( (TargetPoint - Parent.Position).LengthSquared < AttackRadius * AttackRadius && !Parent.IsKnocked)
                {
                    if (_secondAttackCooldown <= 0)
                    {
                        base.Orientate(TargetPoint);
                        _secondAttackCooldown = 4.5f;
                        _leftWeapon.Attack2(Parent);
                    }
                    else if (_firstAttackCooldown <= 0)
                    {
                        base.Orientate(TargetPoint);
                        _firstAttackCooldown = 1.5f;
                        _leftWeapon.Attack1(Parent);
                    }
                    base.CancelMovement();
                }
                else
                {
                    base.MoveTo(TargetPoint);
                }
            }
            base.LookTarget();
        }

        private void OnMiss(Projectile Arrow)
        {
            AttackRadius -= Chunk.BlockSize * 2;
            AttackRadius = System.Math.Max(AttackRadius, Chunk.BlockSize * 2);
        }
        
        private void OnHit(Projectile Arrow)
        {
            AttackRadius = DefaultAttackRadius;
        }

        protected override bool UseCollision => true;
    }
}
