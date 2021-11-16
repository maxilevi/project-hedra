using System;
using System.Globalization;
using Hedra.Core;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.WeaponSystem;

namespace Hedra.Engine.SkillSystem.Rogue.Ninja
{
    public class FinishingBlow : SpecialAttackSkill<RogueWeapon>
    {
        private const float AttackPowerModifier = 1.25f;
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/FinishingBlow.png");

        private int HitCount => 5 + (int)(7 * (Level / (float)MaxLevel));
        public override float ManaCost => 25;
        public override float MaxCooldown => 45;
        protected override int MaxLevel => 20;
        public override string Description => Translations.Get("finishing_blow_desc");
        public override string DisplayName => Translations.Get("finishing_blow_skill");

        public override string[] Attributes => new[]
        {
            Translations.Get("finishing_blow_damage_change",
                (HitCount * User.UnRandomizedDamageEquation * AttackPowerModifier).ToString("0.0",
                    CultureInfo.InvariantCulture)),
            Translations.Get("finishing_blow_hit_change", HitCount)
        };

        protected override void DoUse()
        {
            var weapon = (RogueWeapon)User.LeftWeapon;
            var options = AttackOptions.Default;
            User.Movement.CaptureMovement = false;
            User.Movement.Orientate();
            Casting = true;
            var k = 0;
            for (var i = 0; i < HitCount; ++i)
            {
                var j = i;

                bool Condition()
                {
                    return weapon.MeetsRequirements() && k == j;
                }

                if (i < HitCount - 1)
                    TaskScheduler.When(Condition, () =>
                    {
                        LaunchAttack(weapon, options, S =>
                        {
                            weapon.PrimaryAttackAnimationSpeed = S * 2;
                            User.Movement.Move(User.Orientation, weapon.PrimaryAttackDuration);
                            weapon.Attack1(User, options);
                        });
                        k++;
                    });
                else
                    TaskScheduler.When(Condition, () =>
                    {
                        LaunchAttack(weapon, options, S =>
                        {
                            weapon.SecondaryAttackAnimationSpeed = S * 1.25f;
                            User.Movement.Move(User.Orientation, weapon.SecondaryAttackDuration);
                            weapon.Attack2(User, options);
                        });
                        k++;
                    });
            }

            TaskScheduler.When(() => k == HitCount - 1, () =>
            {
                User.Movement.CaptureMovement = true;
                Casting = false;
            });
        }

        private void LaunchAttack(Weapon Weapon, AttackOptions Options, Action<float> Closure)
        {
            var oldSpeed = Weapon.PrimaryAttackAnimationSpeed;
            var oldPower = User.AttackPower;
            try
            {
                User.AttackPower *= AttackPowerModifier;
                Closure(oldSpeed);
            }
            finally
            {
                User.AttackPower = oldPower;
                Weapon.PrimaryAttackAnimationSpeed = oldSpeed;
            }
        }

        protected override void BeforeUse(RogueWeapon Weapon)
        {
            throw new NotImplementedException();
        }
    }
}