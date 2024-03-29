using Hedra.Components.Effects;
using Hedra.Localization;
using Hedra.Rendering;

namespace Hedra.Engine.SkillSystem.Warrior.Paladin
{
    public class Faith : PassiveSkill
    {
        private AttackResistanceBonusComponent _component;
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Faith.png");

        protected override int MaxLevel => 15;
        private float AttackResistanceChange => .3f * (Level / (float)MaxLevel);
        public override string Description => Translations.Get("faith_desc");
        public override string DisplayName => Translations.Get("faith_skill");

        public override string[] Attributes => new[]
        {
            Translations.Get("faith_resistance_change", (int)(AttackResistanceChange * 100))
        };

        protected override void Add()
        {
            User.AddComponent(_component =
                new AttackResistanceBonusComponent(User, User.AttackResistance * AttackResistanceChange));
        }

        protected override void Remove()
        {
            User.RemoveComponent(_component);
            _component = null;
        }
    }
}