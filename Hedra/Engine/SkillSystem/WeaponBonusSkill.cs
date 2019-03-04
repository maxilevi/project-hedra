using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.WeaponSystem;
using OpenTK;

namespace Hedra.Engine.SkillSystem
{
    public abstract class WeaponBonusSkill : CappedSkill
    {
        private Weapon _activeWeapon;
        private bool _isActive;
        
        protected override void DoUse()
        {
            Player.BeforeDamaging += BeforeDamaging;
            Player.AfterDamaging += AfterDamaging;
            Player.AfterAttack += AfterAttack;
            _isActive = true;
        }
        private void AfterDamaging(IEntity Victim, float Damage)
        {
            Player.BeforeDamaging -= BeforeDamaging;
            Player.AfterDamaging -= AfterDamaging;
            Player.AfterAttack -= AfterAttack;
            _isActive = false;
            _activeWeapon.Outline = false;
            _activeWeapon = null;
            SetOnCooldown();
        }

        private void AfterAttack(AttackOptions Options)
        {
            if(_isActive)
                AfterDamaging(default(IEntity), default(float));
        }

        protected abstract void BeforeDamaging(IEntity Victim, float Damage);

        public override void Update()
        {
            base.Update();
            if (_isActive && _activeWeapon != Player.LeftWeapon)
            {
                if (_activeWeapon != null) _activeWeapon.Outline = false;
                _activeWeapon = Player.LeftWeapon;
                _activeWeapon.Outline = true;
                _activeWeapon.OutlineColor = OutlineColor;
            }
        }

        protected abstract Vector4 OutlineColor { get; }
        protected override bool HasCooldown => !ShouldDisable;
        public override float IsAffectingModifier => _isActive ? 1 : 0;
        protected override bool ShouldDisable => _isActive;
    }
}