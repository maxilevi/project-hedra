using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.WeaponSystem;
using System.Numerics;

namespace Hedra.Engine.SkillSystem
{
    public abstract class WeaponBonusSkill : CappedSkill<IPlayer>
    {
        private Weapon _activeWeapon;
        private bool _isAttacking;
        protected bool IsActive { get; set; }
        
        protected override void DoUse()
        {
            User.DamageModifiers += ApplyBonusToEnemyWrapper;
            User.BeforeAttack += BeforeAttack;
            User.AfterAttack += AfterAttack;
            IsActive = true;
        }
        private void AfterDamaging(IEntity Victim, float Damage)
        {
            User.DamageModifiers -= ApplyBonusToEnemyWrapper;
            User.BeforeAttack -= BeforeAttack;
            User.AfterAttack -= AfterAttack;
            IsActive = false;
            _activeWeapon.Outline = false;
            _activeWeapon = null;
            SetOnCooldown();
        }

        private void AfterAttack(AttackOptions Options)
        {
            _isAttacking = false;
            if(IsActive)
                AfterDamaging(default(IEntity), default(float));
        }

        private void BeforeAttack(AttackOptions Options)
        {
            _isAttacking = true;
        }

        private void ApplyBonusToEnemyWrapper(IEntity Victim, ref float Damage)
        {
            if(_isAttacking)
                ApplyBonusToEnemy(Victim, ref Damage);
        }

        protected abstract void ApplyBonusToEnemy(IEntity Victim, ref float Damage);

        public override void Update()
        {
            base.Update();
            if (IsActive && _activeWeapon != User.LeftWeapon)
            {
                if (_activeWeapon != null) _activeWeapon.Outline = false;
                _activeWeapon = User.LeftWeapon;
                _activeWeapon.Outline = true;
                _activeWeapon.OutlineColor = OutlineColor;
            }
        }

        protected abstract Vector4 OutlineColor { get; }
        protected override bool HasCooldown => !ShouldDisable;
        public override float IsAffectingModifier => IsActive ? 1 : 0;
        protected override bool ShouldDisable => IsActive;
    }
}