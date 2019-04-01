using System.Globalization;
using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.WeaponSystem;

namespace Hedra.Engine.SkillSystem.Rogue.Assassin
{
    public class CleanCut : SpecialAttackPassiveSkill<RogueWeapon>
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/CleanCut.png");
        
        protected override void BeforeUse(RogueWeapon Weapon, AttackOptions Options)
        {
            Player.AfterDamaging += AfterDamaging;
        }

        protected override void AfterUse(RogueWeapon Weapon, AttackOptions Options)
        {
            Player.AfterDamaging -= AfterDamaging;
        }

        private void AfterDamaging(IEntity Victim, float Amount)
        {
            if (Utils.Rng.NextFloat() < Chance && Victim.SearchComponent<BleedingComponent>() == null)
            {
                Victim.AddComponent(new BleedingComponent(Victim, Player, 5, Damage));
            }
        }

        protected override int MaxLevel => 15;
        private float Chance => .1f + .1f * (Level / (float)MaxLevel);
        private float Damage => 20f + 40f * (Level / (float) MaxLevel);
        public override string Description => Translations.Get("clean_cut_desc");
        public override string DisplayName => Translations.Get("clean_cut_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("clean_cut_chance_change", (int)(Chance * 100)),
            Translations.Get("clean_cut_damage_change", Damage.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}