using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.WeaponSystem;

namespace Hedra.Engine.SkillSystem
{
    public abstract class SpecialAttackSkill<T> : CappedSkill<IPlayer> where T : Weapon
    {
        protected override bool ShouldDisable => !User.HasWeapon || !(User.LeftWeapon is T weapon) || !weapon.PrimaryAttackEnabled;

        protected override void DoUse()
        {
            var weapon = (T) User.LeftWeapon;
            BeforeUse(weapon);
            weapon.Attack1(User);
        }

        public override void Update()
        {        
        }

        protected abstract void BeforeUse(T Weapon);
    }
}