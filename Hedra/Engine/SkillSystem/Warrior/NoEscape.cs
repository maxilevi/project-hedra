using System.Globalization;
using Hedra.Components.Effects;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Localization;
using Hedra.Rendering;

namespace Hedra.Engine.SkillSystem.Warrior
{
    public class NoEscape : ConditionedPassiveSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/NoEscape.png");
        private SpeedBonusComponent _component;
        
        protected override void DoAdd()
        {
            User.AddComponent(_component = new SpeedBonusComponent(User, User.Speed * SpeedChange));
        }

        protected override void DoRemove()
        {
            if(_component != null) User.RemoveComponent(_component);
            _component = null;
        }

        protected override bool CheckIfCanDo()
        {
            var entities = World.Entities;
            for (var i = 0; i < entities.Count; ++i)
            {
                if (entities[i] != User && entities[i].Health < entities[i].MaxHealth * .3f)
                    return true;
            }
            return false;
        }

        private float SpeedChange => .1f + .3f * (Level / (float) MaxLevel);
        private float SearchRange => 64 + 80 * (Level / (float) MaxLevel);
        protected override int MaxLevel => 10;
        public override string Description => Translations.Get("no_escape_desc");
        public override string DisplayName => Translations.Get("no_escape_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("no_escape_speed_change", (int) (SpeedChange * 100)),
            Translations.Get("no_escape_range_change", SearchRange.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}