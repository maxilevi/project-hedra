using System;
using System.Drawing;
using System.Globalization;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;
using Hedra.Rendering.Particles;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Warrior.Paladin
{
    public class Salvation : DurationSingleAnimationSkillWithStance
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Salvation.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSalvation.dae");
        protected override Animation StanceAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSalvationStance.dae");
        protected override bool EquipWeapons => false;
        private float _targetBloom;

        protected override void DoStart()
        {
            base.DoStart();
            World.HighlightArea(Player.Position, EffectColor, Radius * 2f, Duration);
            GameSettings.BloomModifier = 8.0f;
        }

        protected override void OnDamageInterval()
        {
            SkillUtils.DoNearby(Player, Radius, -1, OnHeal);
            World.Particles.VariateUniformly = true;
            World.Particles.GravityEffect = .25f;
            World.Particles.Scale = Vector3.One;
            World.Particles.ScaleErrorMargin = new Vector3(.25f, .25f, .25f);
            World.Particles.PositionErrorMargin = new Vector3(4f, .5f, 4f);
            World.Particles.Shape = ParticleShape.Sphere;
            World.Particles.ParticleLifetime = 1.5f;

            for (var i = 0; i < 25; i++)
            {
                World.Particles.Position = Player.Position + new Vector3(Utils.Rng.NextFloat() * 2f - 1f, 0, Utils.Rng.NextFloat() * 2f - 1f) * Radius;
                World.Particles.Direction = (Utils.Rng.NextFloat() * .5f + .5f) * Vector3.UnitY * 4f;
                World.Particles.Color = EffectColor;
                World.Particles.Emit();
            }
        }

        public override void Update()
        {
            base.Update();
            if (IsActive)
            {
                GameSettings.BloomModifier = Mathf.Lerp(GameSettings.BloomModifier, _targetBloom, Time.DeltaTime);
            }
        }

        protected virtual void OnHeal(IEntity Entity, float Dot)
        {
            var healthBonus = HealBonus * (DamageInterval / Duration);
            Player.Health += healthBonus;
            if(!Entity.IsDead && Entity.IsFriendly)
                Entity.Health += healthBonus;
        }

        protected override void DoEnd()
        {
            base.DoEnd();
            GameSettings.BloomModifier = 1.0f;
            TaskScheduler.While(() => Math.Abs(GameSettings.BloomModifier - 1.0f) > .005f, delegate
            {
                GameSettings.BloomModifier = Mathf.Lerp(GameSettings.BloomModifier, 1.0f, Time.IndependantDeltaTime);
            });
            TaskScheduler.When(() => Math.Abs(GameSettings.BloomModifier - 1.0f) < .005f, delegate
            {
                GameSettings.BloomModifier = 1.0f;
            });
        }

        private Vector4 EffectColor => Color.Gold.ToVector4() * 1.5f;
        protected override float Duration => 5 + 5 * (Level / (float) MaxLevel);
        protected override int MaxLevel => 10;
        protected virtual float Radius => 16 + 16 * (Level / (float) MaxLevel);
        protected float HealBonus => 40 + 80 * (Level / (float) MaxLevel);
        public override float MaxCooldown => 74 - Level;
        public override float ManaCost => 80;
        public override string Description => Translations.Get("salvation_desc");
        public override string DisplayName => Translations.Get("salvation_skill");
        public override string[] Attributes => new []
        {
            Translations.Get("salvation_area_change", Radius.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("salvation_heal_change", HealBonus.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}