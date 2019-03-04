using System.Globalization;
using Hedra.Components.Effects;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Warrior.Paladin
{
    public class FireEnchant : WeaponBonusSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/FireEnchant.png");

        protected override void BeforeDamaging(IEntity Victim, float Damage)
        {
            if (Victim.SearchComponent<BurningComponent>() == null)
            {
                Victim.AddComponent(new BurningComponent(Victim, Player, 5f, Damage));
            }
        }

        protected override Vector4 OutlineColor => Colors.OrangeRed;
        protected override int MaxLevel => 15;
        private float Damage => 15 + 30 * (Level / (float) MaxLevel);
        public override string Description => Translations.Get("fire_enchant_desc");
        public override string DisplayName => Translations.Get("fire_enchant_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("fire_enchant_damage_change", Damage.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}