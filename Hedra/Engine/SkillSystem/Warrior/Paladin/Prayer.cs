using System;
using SixLabors.ImageSharp;
using SixLabors.Fonts;
using System.Globalization;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.Rendering.Particles;
using Hedra.Sound;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.SkillSystem.Warrior.Paladin
{
    public class Prayer : DurationSingleAnimationSkillWithStance
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Prayer.png");

        protected override Animation SkillAnimation { get; } =
            AnimationLoader.LoadAnimation("Assets/Chr/WarriorPrayer.dae");

        protected override Animation StanceAnimation { get; } =
            AnimationLoader.LoadAnimation("Assets/Chr/WarriorPrayerStance.dae");

        protected override bool EquipWeapons => false;
        private float _targetBloom;

        protected override void DoStart()
        {
            base.DoStart();
            _targetBloom = 8.0f;
            SkillUtils.DoNearby(User, Radius, -1, OnHeal);
            SoundPlayer.PlaySound(SoundType.HealSound, User.Position);
        }

        protected virtual void OnHeal(IEntity Entity, float Dot)
        {
            User.Health += HealBonus;
            if (!Entity.IsDead && Entity.IsFriendly)
            {
                Entity.Health += HealBonus;
            }
            else if (!Entity.IsFriendly)
            {
                Entity.Damage(Damage, User, out var xp);
                User.XP += xp;
            }
        }

        public override void Update()
        {
            base.Update();
            if (IsActive)
                GameSettings.BloomModifier = Mathf.Lerp(GameSettings.BloomModifier, _targetBloom, Time.DeltaTime);
        }

        protected override void DoEnd()
        {
            base.DoEnd();
            TaskScheduler.While(() => Math.Abs(GameSettings.BloomModifier - 1.0f) > .005f,
                delegate
                {
                    GameSettings.BloomModifier =
                        Mathf.Lerp(GameSettings.BloomModifier, 1.0f, Time.IndependentDeltaTime);
                });
            TaskScheduler.When(() => Math.Abs(GameSettings.BloomModifier - 1.0f) < .005f,
                delegate { GameSettings.BloomModifier = 1.0f; });
        }

        protected override float Duration => .5f;
        protected override int MaxLevel => 10;
        protected virtual float Radius => 16 + 16 * (Level / (float)MaxLevel);
        private float HealBonus => 48 + 80 * (Level / (float)MaxLevel);
        private float Damage => HealBonus * .5f;
        public override float MaxCooldown => 54 - Level;
        public override float ManaCost => 80;
        public override string Description => Translations.Get("prayer_desc");
        public override string DisplayName => Translations.Get("prayer_skill");

        public override string[] Attributes => new[]
        {
            Translations.Get("prayer_area_change", Radius.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("prayer_heal_change", HealBonus.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("prayer_damage_change", Damage.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}