using System;
using System.Globalization;
using Hedra.Engine.Management;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.WeaponSystem;

namespace Hedra.Engine.SkillSystem.Rogue.Ninja
{
    public class FinishingBlow : SpecialAttackSkill<RogueWeapon>
    {
        private const float AttackPowerModifier = 1.25f;
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/FinishingBlow.png");

        protected override void DoUse()
        {
            var weapon = (RogueWeapon) Player.LeftWeapon;
            var options = AttackOptions.Default;
            Player.Movement.CaptureMovement = false;
            Player.Movement.Orientate();
            Casting = true;
            var k = 0;
            for (var i = 0; i < HitCount; ++i)
            {
                var j = i;
                bool Condition() => weapon.MeetsRequirements() && k == j;
                if(i < HitCount-1) TaskScheduler.When(Condition, () =>
                {
                    LaunchAttack(weapon, options, S =>
                    {
                        weapon.PrimaryAttackAnimationSpeed = S * 2;
                        Player.Movement.Move(Player.Orientation, weapon.PrimaryAttackDuration);
                        weapon.Attack1(Player, options);
                    });
                    k++;
                });
                else TaskScheduler.When(Condition, () =>
                {
                    LaunchAttack(weapon, options, S =>
                    {
                        weapon.SecondaryAttackAnimationSpeed = S * 1.25f;
                        Player.Movement.Move(Player.Orientation, weapon.SecondaryAttackDuration);
                        weapon.Attack2(Player, options);
                    });
                    k++;
                });
            }
            TaskScheduler.When(() => k == HitCount-1, () =>
            {
                Player.Movement.CaptureMovement = true;
                Casting = false;
            });
        }

        private void LaunchAttack(Weapon Weapon, AttackOptions Options, Action<float> Closure)
        {
            var oldSpeed = Weapon.PrimaryAttackAnimationSpeed;
            var oldPower = Player.AttackPower;
            try
            {
                Player.AttackPower *= AttackPowerModifier;
                Closure(oldSpeed);
            }
            finally
            {
                Player.AttackPower = oldPower;
                Weapon.PrimaryAttackAnimationSpeed = oldSpeed;
            }
        }

        private int HitCount => 5 + (int) (7 * (Level / (float)MaxLevel));
        public override float ManaCost => 25;
        public override float MaxCooldown => 45;
        protected override int MaxLevel => 20;
        protected override void BeforeUse(RogueWeapon Weapon) => throw new NotImplementedException();
        public override string Description => Translations.Get("finishing_blow_desc");
        public override string DisplayName => Translations.Get("finishing_blow_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("finishing_blow_damage_change", (HitCount * Player.UnRandomizedDamageEquation * AttackPowerModifier).ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("finishing_blow_hit_change", HitCount)
        };
    }
}