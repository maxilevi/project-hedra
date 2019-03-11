using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.WeaponSystem;
using OpenTK;

namespace Hedra.Engine.SkillSystem
{
    public abstract class WeaponBonusSkill : CappedSkill
    {
        private Weapon _activeWeapon;
        protected bool IsActive { get; set; }
        
        protected override void DoUse()
        {
            Player.DamageModifiers += ApplyBonusToEnemy;
            Player.AfterDamaging += AfterDamaging;
            Player.AfterAttack += AfterAttack;
            IsActive = true;
        }
        private void AfterDamaging(IEntity Victim, float Damage)
        {
            Player.DamageModifiers -= ApplyBonusToEnemy;
            Player.AfterDamaging -= AfterDamaging;
            Player.AfterAttack -= AfterAttack;
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
            if (IsActive && _activeWeapon != Player.LeftWeapon)
            {
                if (_activeWeapon != null) _activeWeapon.Outline = false;
                _activeWeapon = Player.LeftWeapon;
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