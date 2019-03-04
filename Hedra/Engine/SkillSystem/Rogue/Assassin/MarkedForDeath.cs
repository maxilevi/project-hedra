using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.WeaponSystem;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Rogue.Assassin
{
    public class MarkedForDeath : WeaponBonusSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/MarkedForDeath.png");

        private class MarkedForDeathComponent : EntityComponent
        {
            public MarkedForDeathComponent(IEntity Entity, float AttackResistanceBonus) : base(Entity)
            {
                Entity.ShowIcon(CacheItem.DeathIcon);
                Entity.AttackResistance -= AttackResistanceBonus;
            }

            public override void Update()
            {
            }
        }
        
        protected override void ApplyBonusToEnemy(IEntity Victim, ref float Damage)
        {
            if (Victim.SearchComponent<MarkedForDeathComponent>() == null)
            {
                Victim.AddComponent(new MarkedForDeathComponent(Victim, AttackResistanceBonus));
            }
        }

        protected override Vector4 OutlineColor => Colors.Red;
        private float AttackResistanceBonus => .2f + .3f * (Level / (float) MaxLevel);
        protected override int MaxLevel => 15;
        public override float ManaCost => 40;
        public override float MaxCooldown => 87;
        public override string Description => Translations.Get("marked_for_death_desc");
        public override string DisplayName => Translations.Get("marked_for_death_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("marked_for_death_resistance_change", (int)(AttackResistanceBonus * 100))
        };
    }
}