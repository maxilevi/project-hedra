using Hedra.Components.Effects;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Warrior.Berserker
{
    public class IronSkin : PassiveSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/IronSkin.png");
        private AttackResistanceBonusComponent _component;
        
        protected override void Add()
        {
            Player.AddComponent(_component = new AttackResistanceBonusComponent(Player, Player.AttackResistance * ResistanceChange));
        }

        protected override void Remove()
        {
            Player.RemoveComponent(_component);
            _component = null;
        }

        protected override int MaxLevel => 15;
        private float ResistanceChange => .3f * (Level / (float) MaxLevel);
        public override string Description => Translations.Get("iron_skin_desc");
        public override string DisplayName => Translations.Get("iron_skin_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("iron_skin_resistance_change", (int)(ResistanceChange * 100))
        };
    }
}