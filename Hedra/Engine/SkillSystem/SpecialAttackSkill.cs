using Hedra.WeaponSystem;

namespace Hedra.Engine.SkillSystem
{
    public abstract class SpecialAttackSkill<T> : BaseSkill where T : Weapon
    {
        protected override bool Grayscale => !Player.HasWeapon || !(Player.LeftWeapon is T weapon) || !weapon.CanDoAttack1;
        
        public override void Use()
        {
            var weapon = (T) Player.LeftWeapon;
            this.BeforeUse(weapon);
            weapon.Attack1(Player);
        }

        public override void Update()
        {        
        }

        protected abstract void BeforeUse(T Weapon);
        
        public override bool MeetsRequirements()
        {
            return base.MeetsRequirements() && !Grayscale;
        }
    }
}