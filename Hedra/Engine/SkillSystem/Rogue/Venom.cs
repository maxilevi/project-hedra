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
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.WeaponSystem;

namespace Hedra.Engine.SkillSystem.Rogue
{
    /// <summary>
    /// Description of Resistance.
    /// </summary>
    public class Venom : SpecialAttackPassiveSkill<RogueWeapon>
    {
        protected override void BeforeUse(RogueWeapon Weapon, AttackOptions Options)
        {
            User.AfterDamaging += AfterDamaging;
        }

        protected override void AfterUse(RogueWeapon Weapon, AttackOptions Options)
        {
            User.AfterDamaging -= AfterDamaging;
        }
        
        private void AfterDamaging(IEntity Victim, float Amount)
        {
            if (Utils.Rng.NextFloat() < Chance && Victim.SearchComponent<PoisonComponent>() == null)
            {
                Victim.AddComponent(new PoisonComponent(Victim, User, 5, Damage));
            }
        }
        
        protected override int MaxLevel => 15;
        private float Chance => .1f + .1f * (Level / (float)MaxLevel);
        private float Duration => 3f + 3f * (Level / (float)MaxLevel);
        private float Damage => 15f + 30f * (Level / (float) MaxLevel);
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Venom.png");
        public override string Description => Translations.Get("venom_desc");
        public override string DisplayName => Translations.Get("venom_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("venom_chance_change", (int)(Chance * 100)),
            Translations.Get("venom_damage_change", Damage.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("venom_duration_change", Duration.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}
