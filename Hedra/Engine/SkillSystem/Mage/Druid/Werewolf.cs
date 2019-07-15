using System.Globalization;
using Hedra.Components.Effects;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.WeaponSystem;

namespace Hedra.Engine.SkillSystem.Mage.Druid
{
    public class Werewolf : MorphSkill
    {
        private AttackSpeedBonusComponent _attackSpeed;
        private AttackPowerBonusComponent _attackPower;
        private SpeedBonusComponent _speedBonus;
        
        protected override void AddEffects()
        {
            User.AddComponent(_attackSpeed = new AttackSpeedBonusComponent(User, User.AttackSpeed * AttackSpeedMultiplier));
            User.AddComponent(_attackPower = new AttackPowerBonusComponent(User, User.AttackPower * AttackPowerMultiplier));
            User.AddComponent(_speedBonus = new SpeedBonusComponent(User, User.Speed * SpeedMultiplier));
        }

        protected override void RemoveEffects()
        {
            User.RemoveComponent(_attackSpeed);
            User.RemoveComponent(_attackPower);
            User.RemoveComponent(_speedBonus);
            _speedBonus = null;
            _attackPower = null;
            _attackSpeed = null;
        }

        private float AttackSpeedMultiplier => .15f  + .6f * (Level / (float)MaxLevel);
        private float AttackPowerMultiplier => .4f  + .85f * (Level / (float)MaxLevel);
        private float SpeedMultiplier => .15f +  .3f * (Level / (float)MaxLevel);
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Werewolf.png");
        public override string Description => Translations.Get("werewolf_desc");
        public override string DisplayName => Translations.Get("werewolf_skill");
        public override float ManaCost => 70;
        protected override HumanType Type => HumanType.WerewolfMorph;
        protected override bool RestrictWeapons => true;
        protected override bool CanUseOtherSkills => false;
        protected override Weapon CustomWeapon => new WerewolfHands();

        public override string[] Attributes => new []
        {
            Translations.Get("morph_time_change", Duration.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("werewolf_damage_change", (int)(AttackPowerMultiplier * 100)),
            Translations.Get("werewolf_attack_speed_change", (int)(AttackSpeedMultiplier * 100)),
            Translations.Get("werewolf_speed_change", (int)(SpeedMultiplier * 100))
        };

        private class WerewolfHands : Hands
        {
            protected override string AttackStanceName => "Assets/Chr/RogueBlade-Stance.dae";
            protected override float PrimarySpeed => 2.0f;
            protected override string[] PrimaryAnimationsNames => new []
            {
                "Assets/Chr/RogueBladeLeftAttack.dae",
                "Assets/Chr/RogueBladeRightAttack.dae"
            };
            protected override float SecondarySpeed => 1.75f;
            protected override string[] SecondaryAnimationsNames => new []
            {
                "Assets/Chr/RogueBladeDoubleAttack.dae"
            }; 
        }
    }
}