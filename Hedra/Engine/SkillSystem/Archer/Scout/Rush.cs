using System.Drawing;
using System.Globalization;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;

namespace Hedra.Engine.SkillSystem.Archer.Scout
{
    public class Rush : ActivateDurationSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Rush.png");
        private InfiniteStaminaComponent _staminaComponent;
        
        protected override void DoEnable()
        {
            Player.Model.Outline = true;
            Player.Model.OutlineColor = Colors.Yellow;
            Player.AddComponent(_staminaComponent = new InfiniteStaminaComponent(Player));
        }

        protected override void DoDisable()
        {
            Player.Model.Outline = false;
            Player.RemoveComponent(_staminaComponent);
            _staminaComponent = null;
        }

        protected override float Duration => 5f + (Level / 15f) * 8f;
        protected override int MaxLevel => 15;
        public override float ManaCost => 80;
        protected override float CooldownDuration => 32 - Level / (float) MaxLevel * 8;
        public override string Description => Translations.Get("rush_desc", Duration.ToString("0.0", CultureInfo.InvariantCulture));
        public override string DisplayName => Translations.Get("rush_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("ruse_time_change", Duration.ToString("0.0", CultureInfo.InvariantCulture))
        };
        
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