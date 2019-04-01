using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Mage
{
    public class FireMastery : PassiveSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/FireImmunity.png");
        private float _previousValue;
        
        protected override void Add()
        {
            Player.Attributes.FireDamageMultiplier += _previousValue = DamageChange;
        }

        protected override void Remove()
        {
            Player.Attributes.FireDamageMultiplier -= _previousValue;
            _previousValue = 0;
        }
        
        protected override int MaxLevel => 15;
        private float DamageChange => .25f + 3.25f * (Level / (float) MaxLevel);
        public override string Description => Translations.Get("fire_mastery_desc");
        public override string DisplayName => Translations.Get("fire_mastery_skill");
        public override string[] Attributes => new []
        {
            Translations.Get("fire_mastery_damage_change", (int) (100 * DamageChange))
        };
    }
}