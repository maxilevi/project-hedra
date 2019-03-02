using Hedra.Engine.Management;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Rogue.Assassin
{
    public class Treason : ConditionedPassiveSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Treason.png");
        
        protected override void DoAdd()
        {
            Player.DamageModifiers += DamageModifier;
        }

        protected override void DoRemove()
        {
            Player.DamageModifiers -= DamageModifier;
        }

        private void DamageModifier(IEntity Victim, ref float Damage)
        {
            if (SkillUtils.IsBehind(Player, Victim))
            {
                Damage *= 1 + DamageBonus;
            }
        }

        protected override bool CheckIfCanDo()
        {
            return SkillUtils.IsBehindAny(Player);
        }
        
        protected override int MaxLevel => 15;
        private float DamageBonus => 1.5f + 2.5f * (Level / (float) MaxLevel);
        public override string Description => Translations.Get("treason_desc");
        public override string DisplayName => Translations.Get("treason_skill");
        public override string[] Attributes => new []
        {
            Translations.Get("treason_damage_change", (int) (DamageBonus * 100))
        };
    }
}