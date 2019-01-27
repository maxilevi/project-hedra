using Hedra.Engine.Player;
using Hedra.Engine.Rendering.UI;
using Hedra.EntitySystem;
using Hedra.WeaponSystem;
using OpenTK;

namespace Hedra.Engine.SkillSystem
{
    public abstract class SpecialAttackPassiveSkill<T> : PassiveSkill where T : Weapon
    {
        public override void Initialize(Vector2 Position, Vector2 Scale, Panel InPanel, IPlayer Player)
        {
            base.Initialize(Position, Scale, InPanel, Player);
            Player.BeforeAttacking += BeforeAttacking;
        }

        private void BeforeAttacking(IEntity Parent, float Damage)
        {
            if(Level > 0 && Player.HasWeapon && Player.LeftWeapon is T weapon)
                BeforeUse(weapon);
        }
        
        protected override void OnChange()
        {
        }
        
        protected abstract void BeforeUse(T Weapon);
    }
}