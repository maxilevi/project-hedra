/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:14 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Components.Effects;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.WeaponSystem;

namespace Hedra.Engine.SkillSystem.Archer
{
    /// <summary>
    /// Description of Resistance.
    /// </summary>
    public class Puncture : SpecialAttackPassiveSkill<Bow>
    {
        protected override int MaxLevel => 25;
        
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/PierceArrows.png");

        protected override void BeforeUse(Bow Weapon)
        {
            void HandlerLambda(Projectile A) => PierceModifier(Weapon, A, HandlerLambda);
            Weapon.BowModifiers += HandlerLambda;
        }

        private void PierceModifier(Bow Weapon, Projectile ArrowProj, OnArrowEvent Lambda)
        {
            ArrowProj.HitEventHandler += delegate(Projectile Sender, IEntity Hit)
            {
                if(Utils.Rng.Next(0, 10) == 0 && Hit.SearchComponent<BleedingComponent>() == null)
                {
                    Hit.AddComponent( new BleedingComponent(Hit, Player, 2 + Level / 10.0f, Player.DamageEquation * (.75f + Level / 10f)) );
                }
            };
            Weapon.BowModifiers -= Lambda;
        }

        public override string Description => Translations.Get("puncture_skill_desc");
        public override string DisplayName => Translations.Get("puncture_skill");
    }
}
