using Hedra.WeaponSystem;

namespace Hedra.Engine.SkillSystem
{
    public abstract class SpecialAttackSkill<T> : BaseSkill where T : Weapon
    {
        protected override bool ShouldDisable => !Player.HasWeapon || !(Player.LeftWeapon is T weapon) || !weapon.PrimaryAttackEnabled;
        
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
    }
}