using Hedra.Components.Effects;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Rogue.Ninja
{
    public class MartialArtsMaster : PassiveSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/MartialArtsMaster.png");
        private AttackResistanceBonusComponent _previousResistance;
        private AttackSpeedBonusComponent _previousAttackSpeed;
        
        protected override void Add()
        {
            Player.AddComponent(_previousResistance = new AttackResistanceBonusComponent(Player, ResistanceChange));
            Player.AddComponent(_previousAttackSpeed = new AttackSpeedBonusComponent(Player, ResistanceChange));
        }
        
        protected override void Remove()
        {
            Player.RemoveComponent(_previousResistance);
            Player.RemoveComponent(_previousAttackSpeed);
        }

        private float ResistanceChange => .05f + .15f * (Level / (float)MaxLevel);
        private float AttackSpeedChange => .1f + .25f * (Level / (float)MaxLevel);
        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("martial_arts_master_desc");
        public override string DisplayName => Translations.Get("martial_arts_master_skill");
        public override string[] Attributes => new []
        {
            Translations.Get("martial_arts_training_resistance_change", (int)(ResistanceChange * 100)),
            Translations.Get("martial_arts_training_attack_speed_change", (int)(AttackSpeedChange * 100))
        };
    }
}