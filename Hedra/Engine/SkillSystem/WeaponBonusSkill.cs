using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.WeaponSystem;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.SkillSystem
{
    public abstract class WeaponBonusSkill : CappedSkill<IPlayer>
    {
        private Weapon _activeWeapon;
        protected bool IsActive { get; set; }
        
        protected override void DoUse()
        {
            User.DamageModifiers += ApplyBonusToEnemy;
            User.AfterDamaging += AfterDamaging;
            User.AfterAttack += AfterAttack;
            IsActive = true;
        }
        private void AfterDamaging(IEntity Victim, float Damage)
        {
            User.DamageModifiers -= ApplyBonusToEnemy;
            User.AfterDamaging -= AfterDamaging;
            User.AfterAttack -= AfterAttack;
            IsActive = false;
            _activeWeapon.Outline = false;
            _activeWeapon = null;
            SetOnCooldown();
        }

        private void AfterAttack(AttackOptions Options)
        {
            if(IsActive)
                AfterDamaging(default(IEntity), default(float));
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