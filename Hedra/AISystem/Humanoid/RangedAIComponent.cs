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
using Hedra.WorldObjects;
using System.Numerics;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.AISystem.Humanoid
{
    /// <summary>
    /// Description of ArcherAI.
    /// </summary>
    public class RangedAIComponent : CombatAIComponent
    {
        private const int DefaultAttackRadius = 96;
        private float _secondAttackCooldown;
        private float _firstAttackCooldown;
        private float _attackRadius = DefaultAttackRadius;
        private readonly RangedWeapon _leftWeapon;
        protected override float SearchRadius => 128;
        protected override float AttackRadius => _attackRadius;
        protected override float ForgetRadius => 192;

        public RangedAIComponent(IHumanoid Parent, bool IsFriendly) : base(Parent, IsFriendly)
        {
            _leftWeapon = (RangedWeapon) Parent.LeftWeapon;
            _leftWeapon.Miss += OnMiss;
            _leftWeapon.Hit += OnHit;
            _leftWeapon.BowModifiers += BowModifiers;
        }

        protected override void DoUpdate()
        {        
            _secondAttackCooldown -= Time.DeltaTime;
            _firstAttackCooldown -= Time.DeltaTime;
            if (ChasingTarget != null && Parent.Physics.StaticRaycast(ChasingTarget.Position + Vector3.UnitY * ChasingTarget.Model.Height * .5f))
            {
                ReduceRange();
            }
        }

        protected override void OnAttack()
        {    
            if (_secondAttackCooldown <= 0 && CanUseSecondAttack)
            {
                _secondAttackCooldown = SecondAttackCooldownTime;
                _leftWeapon.Attack2(Parent, new AttackOptions
                {
                    IgnoreEntities = IgnoreEntities
                });
            }
            else if (_firstAttackCooldown <= 0 && CanUseFirstAttack)
            {
                _firstAttackCooldown = 1.5f;
                _leftWeapon.Attack1(Parent, new AttackOptions
                {
                    IgnoreEntities = IgnoreEntities
                });
            }
        }

        private void OnMiss(Projectile Arrow)
        {
            ReduceRange();
        }
        
        private void ReduceRange()
        {
            _attackRadius -= Chunk.BlockSize * 2;
            _attackRadius = System.Math.Max(AttackRadius, Chunk.BlockSize * 2);
        }
        
        private void OnHit(Projectile Arrow)
        {
            _attackRadius = DefaultAttackRadius;
        }

        private void BowModifiers(Projectile Arrow)
        {
            Arrow.Position -= Vector3.UnitY;
        }

        protected override bool UseCollision => true;
        protected virtual bool CanUseFirstAttack => true;
        protected virtual bool CanUseSecondAttack => true;
        protected virtual float SecondAttackCooldownTime => 4.5f;
    }
}
