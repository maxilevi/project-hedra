using Hedra.Components.Effects;
using Hedra.Localization;
using Hedra.Rendering;

namespace Hedra.Engine.SkillSystem.Rogue.Ninja
{
    public class MartialArtsTraining : PassiveSkill
    {
        private AttackPowerBonusComponent _attackPowerBonusComponent;
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/MartialArtsTraining.png");

        private float DamageChange => .05f + .2f * (Level / (float)MaxLevel);
        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("martial_arts_training_desc");
        public override string DisplayName => Translations.Get("martial_arts_training_skill");

        public override string[] Attributes => new[]
        {
            Translations.Get("martial_arts_training_damage_change", (int)(DamageChange * 100))
        };

        protected override void Add()
        {
            User.AddComponent(_attackPowerBonusComponent = new AttackPowerBonusComponent(User, DamageChange));
        }

        protected override void Remove()
        {
            User.RemoveComponent(_attackPowerBonusComponent);
        }
    }
}