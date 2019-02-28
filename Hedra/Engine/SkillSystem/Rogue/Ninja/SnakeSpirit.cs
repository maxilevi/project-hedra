using System.Globalization;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Rogue.Ninja
{
    public class SnakeSpirit : ActivateSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/SnakeSpirit.png");
        
        protected override void DoEnable()
        {
            Player.Model.Outline = true;
            Player.Model.OutlineColor = Colors.GreenYellow;
            Player.SearchComponent<DamageComponent>().MissChance = DodgeChance;
        }

        protected override void DoDisable()
        {
            Player.Model.Outline = false;
            Player.SearchComponent<DamageComponent>().MissChance = DamageComponent.DefaultMissChance;
        }

        private float DodgeChance => .15f + .6f * (Level / (float) MaxLevel);
        protected override float Duration => 4.5f + 4f * (Level / (float) MaxLevel);
        protected override int MaxLevel => 15;
        public override float ManaCost => 60;
        public override float MaxCooldown => 28;
        public override string Description => Translations.Get("snake_spirit_desc");
        public override string DisplayName => Translations.Get("snake_spirit_skill");
        public override string[] Attributes => new []
        {
            Translations.Get("snake_spirit_duration_change", Duration.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("snake_spirit_dodge_change", (int) (DodgeChance * 100))
        };
    }
}