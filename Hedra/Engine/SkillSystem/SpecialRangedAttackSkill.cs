using Hedra.EntitySystem;
using Hedra.WeaponSystem;
using Hedra.WorldObjects;

namespace Hedra.Engine.SkillSystem
{
    public abstract class SpecialRangedAttackSkill : SpecialAttackSkill<RangedWeapon>
    {
        protected sealed override void BeforeUse(RangedWeapon Weapon)
        {
            void HandlerLambda(Projectile A)
            {
                ModifierHandler(Weapon, A, HandlerLambda);
            }

            Weapon.BowModifiers += HandlerLambda;
        }

        private void ModifierHandler(RangedWeapon Weapon, Projectile Arrow, OnProjectileEvent Event)
        {
            Arrow.MoveEventHandler += OnMove;
            Arrow.LandEventHandler += OnLand;
            Arrow.HitEventHandler += OnHit;
            Weapon.BowModifiers -= Event;
        }

        protected virtual void OnLand(Projectile Proj, LandType Type)
        {
        }

        protected virtual void OnHit(Projectile Proj, IEntity Victim)
        {
        }

        protected virtual void OnMove(Projectile Proj)
        {
        }
    }
}