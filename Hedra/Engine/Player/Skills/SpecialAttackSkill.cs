using System;
using Hedra.Engine.Player.ToolbarSystem;
using Hedra.WeaponSystem;

namespace Hedra.Engine.Player.Skills
{
    public abstract class SpecialAttackSkill<T> : BaseSkill where T : Weapon
    {
        protected override bool Grayscale => !Player.HasWeapon || !(Player.LeftWeapon is T);
        
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