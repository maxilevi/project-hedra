/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:14 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Globalization;
using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.WeaponSystem;
using Hedra.WorldObjects;

namespace Hedra.Engine.SkillSystem.Archer
{
    /// <summary>
    /// Description of Resistance.
    /// </summary>
    public class Puncture : SpecialAttackPassiveSkill<Bow>
    {
        protected override int MaxLevel => 25;
        private float TotalDamage => User.DamageEquation * (.75f + Level / 10f);
        private float TotalTime => 2 + Level / 10.0f;
        private float BleedChance => .1f;
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/PierceArrows.png");

        protected override void BeforeUse(Bow Weapon, AttackOptions Options)
        {
            if (Options.Charge < .35f) return;
            Weapon.BowModifiers += PierceModifier;
        }

        protected override void AfterUse(Bow Weapon, AttackOptions Options)
        {
            Weapon.BowModifiers -= PierceModifier;
        }
        
        private void PierceModifier(Projectile ArrowProj)
        {
            ArrowProj.HitEventHandler += delegate(Projectile Sender, IEntity Hit)
            {
                if(Utils.Rng.NextFloat() < BleedChance && Hit.SearchComponent<BleedingComponent>() == null)
                {
                    Hit.AddComponent( new BleedingComponent(Hit, User, TotalTime, TotalDamage));
                }
            };
        }

        public override string Description => Translations.Get("puncture_skill_desc");
        public override string DisplayName => Translations.Get("puncture_skill");
        public override string[] Attributes => new []
        {
            Translations.Get("puncture_damage_change", TotalDamage.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("puncture_bleed_time_change", TotalTime.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("puncture_bleed_change", (int)(BleedChance * 100))
        };
    }
}
