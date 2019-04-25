using Hedra.Engine.Management;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Rogue.Assassin
{
    public class Treason : ConditionedPassiveSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Treason.png");
        
        protected override void DoAdd()
        {
            User.DamageModifiers += DamageModifier;
        }

        protected override void DoRemove()
        {
            User.DamageModifiers -= DamageModifier;
        }

        private void DamageModifier(IEntity Victim, ref float Damage)
        {
            if (SkillUtils.IsBehind(User, Victim))
            {
                Damage *= 1 + DamageBonus;
            }
        }

        protected override bool CheckIfCanDo()
        {
            return SkillUtils.IsBehindAny(User);
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