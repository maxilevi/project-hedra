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
        
        protected override void Add()
        {
            Player.DamageModifiers += DamageModifier;
        }

        protected override void Remove()
        {
            Player.DamageModifiers -= DamageModifier;
        }

        private void DamageModifier(IEntity Victim, ref float Damage)
        {
            if (CanBackStab(Victim))
            {
                Damage *= 1 + DamageBonus;
            }
        }

        protected override bool CheckIfCanDo()
        {
            var entities = World.Entities;
            for (var i = 0; i < entities.Count; ++i)
            {
                if (entities[i] != Player && CanBackStab(entities[i]))
                    return true;
            }
            return false;
        }

        private bool CanBackStab(IEntity Victim)
        {
            return Player.InAttackRange(Victim) && Vector3.Dot(Victim.Orientation, Player.Orientation) > -.9f;
        }
        
        protected override int MaxLevel => 15;
        private float DamageBonus => .5f + 1.5f * (Level / (float) MaxLevel);
        public override string Description => Translations.Get("treason_desc");
        public override string DisplayName => Translations.Get("treason_skill");
        public override string[] Attributes => new []
        {
            Translations.Get("treason_damage_change", (int) (DamageBonus * 100))
        };
    }
}