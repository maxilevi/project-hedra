using Hedra.Components.Effects;
using Hedra.Localization;
using Hedra.Rendering;

namespace Hedra.Engine.SkillSystem.Warrior.Berserker
{
    public class IronSkin : PassiveSkill
    {
        private AttackResistanceBonusComponent _component;
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/IronSkin.png");

        protected override int MaxLevel => 15;
        private float ResistanceChange => .3f * (Level / (float)MaxLevel);
        public override string Description => Translations.Get("iron_skin_desc");
        public override string DisplayName => Translations.Get("iron_skin_skill");

        public override string[] Attributes => new[]
        {
            Translations.Get("iron_skin_resistance_change", (int)(ResistanceChange * 100))
        };

        protected override void Add()
        {
            User.AddComponent(_component =
                new AttackResistanceBonusComponent(User, User.AttackResistance * ResistanceChange));
        }

        protected override void Remove()
        {
            User.RemoveComponent(_component);
            _component = null;
        }
    }
}