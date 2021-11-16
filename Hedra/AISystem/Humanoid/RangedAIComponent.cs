/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 08/12/2016
 * Time: 11:13 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.EntitySystem;
using Hedra.WeaponSystem;
using Hedra.WorldObjects;

namespace Hedra.AISystem.Humanoid
{
    /// <summary>
    ///     Description of ArcherAI.
    /// </summary>
    public class RangedAIComponent : CombatAIComponent
    {
        private const int DefaultAttackRadius = 96;
        private float _attackRadius = DefaultAttackRadius;
        private float _firstAttackCooldown;
        private RangedWeapon _leftWeapon;
        private float _secondAttackCooldown;

        public RangedAIComponent(IHumanoid Parent, bool IsFriendly) : base(Parent, IsFriendly)
        {
        }

        protected override float SearchRadius => 128;
        protected override float AttackRadius => _attackRadius;
        protected override float ForgetRadius => 192;

        protected override bool UseCollision => true;
        protected virtual bool CanUseFirstAttack => true;
        protected virtual bool CanUseSecondAttack => true;
        protected virtual float SecondAttackCooldownTime => 4.5f;

        protected override void DoUpdate()
        {
            if (_leftWeapon != Parent.LeftWeapon)
                SetWeapon((RangedWeapon)Parent.LeftWeapon);
            _secondAttackCooldown -= Time.DeltaTime;
            _firstAttackCooldown -= Time.DeltaTime;
            if (ChasingTarget != null &&
                Parent.Physics.StaticRaycast(ChasingTarget.Position + Vector3.UnitY * ChasingTarget.Model.Height))
                ReduceRange();
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

        private void SetWeapon(RangedWeapon Weapon)
        {
            if (_leftWeapon != null)
            {
                _leftWeapon.Miss -= OnMiss;
                _leftWeapon.Hit -= OnHit;
                _leftWeapon.BowModifiers -= BowModifiers;
            }

            _leftWeapon = Weapon;
            _leftWeapon.Miss += OnMiss;
            _leftWeapon.Hit += OnHit;
            _leftWeapon.BowModifiers += BowModifiers;
        }

        private void OnMiss(Projectile Arrow)
        {
            ReduceRange();
        }

        private void ReduceRange()
        {
            _attackRadius -= Chunk.BlockSize * 2;
            _attackRadius = Math.Max(AttackRadius, Chunk.BlockSize * 2);
        }

        private void OnHit(Projectile Arrow)
        {
            _attackRadius = DefaultAttackRadius;
        }

        private void BowModifiers(Projectile Arrow)
        {
            Arrow.Position -= Vector3.UnitY;
        }
    }
}