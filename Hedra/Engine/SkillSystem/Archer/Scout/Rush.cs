using System.Globalization;
using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Rendering;

namespace Hedra.Engine.SkillSystem.Archer.Scout
{
    public class Rush : PlayerActivateDurationSkill
    {
        private InfiniteStaminaComponent _staminaComponent;
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Rush.png");

        protected override float Duration => 5f + Level / 15f * 8f;
        protected override int MaxLevel => 15;
        public override float ManaCost => 80;
        protected override float CooldownDuration => 32 - Level / (float)MaxLevel * 8;

        public override string Description =>
            Translations.Get("rush_desc", Duration.ToString("0.0", CultureInfo.InvariantCulture));

        public override string DisplayName => Translations.Get("rush_skill");

        public override string[] Attributes => new[]
        {
            Translations.Get("ruse_time_change", Duration.ToString("0.0", CultureInfo.InvariantCulture))
        };

        protected override void DoEnable()
        {
            User.Model.Outline = true;
            User.Model.OutlineColor = Colors.Yellow;
            User.AddComponent(_staminaComponent = new InfiniteStaminaComponent(User));
        }

        protected override void DoDisable()
        {
            User.Model.Outline = false;
            User.RemoveComponent(_staminaComponent);
            _staminaComponent = null;
        }

        private class InfiniteStaminaComponent : Component<IHumanoid>
        {
            public InfiniteStaminaComponent(IHumanoid Entity) : base(Entity)
            {
            }

            public override void Update()
            {
                Parent.Stamina = Parent.MaxStamina;
            }
        }
    }
}