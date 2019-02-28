using Hedra.Components.Effects;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Rogue.Ninja
{
    public class MartialArtsTraining : PassiveSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/MartialArtsTraining.png");
        private AttackPowerBonusComponent _attackPowerBonusComponent;
        
        protected override void Add()
        {
            Player.AddComponent(_attackPowerBonusComponent = new AttackPowerBonusComponent(Player, DamageChange));
        }
        
        protected override void Remove()
        {
            Player.RemoveComponent(_attackPowerBonusComponent);
        }

        private float DamageChange => .05f + .2f * (Level / (float)MaxLevel);
        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("martial_arts_training_desc");
        public override string DisplayName => Translations.Get("martial_arts_training_skill");
        public override string[] Attributes => new []
        {
            Translations.Get("martial_arts_training_damage_change", (int)(DamageChange * 100))
        };
    }
}