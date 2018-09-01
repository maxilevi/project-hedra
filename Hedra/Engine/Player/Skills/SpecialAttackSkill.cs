using System;
using Hedra.Engine.ItemSystem.WeaponSystem;
using Hedra.Engine.Player.ToolbarSystem;

namespace Hedra.Engine.Player.Skills
{
    public abstract class SpecialAttackSkill<T> : BaseSkill where T : Weapon
    {
        protected override bool Grayscale => !Player.HasWeapon || !(Player.Model.LeftWeapon is T);
        
        public override void Use()
        {
            var weapon = (T) Player.Model.LeftWeapon;
            this.BeforeUse(weapon);
            weapon.Attack1(Player);
        }

        public override void Update()
        {        
        }

        protected abstract void BeforeUse(T Weapon);
        
        public override bool MeetsRequirements(IToolbar Bar, int CastingAbilityCount)
        {
            return base.MeetsRequirements(Bar, CastingAbilityCount) && !Grayscale;
        }
    }
}