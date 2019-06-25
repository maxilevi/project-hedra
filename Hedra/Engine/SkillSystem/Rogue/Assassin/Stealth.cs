using Hedra.Components.Effects;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Localization;

namespace Hedra.Engine.SkillSystem.Rogue.Assassin
{
    public class Stealth : PassiveSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Stealth.png");
        private SpeedBonusComponent _component;
        private bool _active;

        protected override void Add()
        {
            _active = true;
            Kill();
        }

        protected override void Remove()
        {
            _active = false;
            Kill();
        }

        public override void Update()
        {
            base.Update();
            if (_active && User.IsInvisible && _component == null)
            {
                User.AddComponent(_component = new SpeedBonusComponent(User, User.Speed * SpeedBonus, false));
            }
            if (_active && !User.IsInvisible && _component != null)
            {
                Kill();
            }
        }

        private void Kill()
        {
            if(_component != null) User.RemoveComponent(_component);
            _component = null;
        }

        protected override int MaxLevel => 15;
        private float SpeedBonus => .1f + .25f * (Level / (float) MaxLevel);
        public override float IsAffectingModifier => _component != null ? 1 : 0;
        public override string Description => Translations.Get("stealth_desc");
        public override string DisplayName => Translations.Get("stealth_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("stealth_speed_change", (int)(SpeedBonus * 100))
        };
    }
}