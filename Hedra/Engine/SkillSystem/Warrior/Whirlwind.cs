/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:13 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Globalization;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.Particles;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Rendering;

namespace Hedra.Engine.SkillSystem.Warrior
{
    /// <summary>
    ///     Description of WeaponThrow.
    /// </summary>
    public class Whirlwind : CappedSkill<IPlayer>
    {
        private readonly TrailRenderer _trail;

        private readonly Animation _whirlwindAnimation;
        private float _frameCounter;
        private float _passedTime;
        private float _rotationY;

        public Whirlwind()
        {
            _trail = new TrailRenderer(() => User.LeftWeapon.WeaponTip, Vector4.One);
            _whirlwindAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorWhirlwind.dae");
            _whirlwindAnimation.OnAnimationEnd += delegate
            {
                if (!Casting) return;
                User.Model.Play(_whirlwindAnimation);
            };
            _whirlwindAnimation.Loop = false;
        }

        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Spin.png");
        protected override bool ShouldDisable => User.Toolbar.DisableAttack || !User.HasWeapon;
        protected override int MaxLevel => 24;
        public override string Description => Translations.Get("whirlwind_desc");
        public override string DisplayName => Translations.Get("whirlwind");
        public override float ManaCost => Math.Max(120 - 4f * Level, 40);
        public override float MaxCooldown => (float)Math.Max(12.0 - .25f * Level, 6) + WhirlwindTime;
        private float Damage => User.DamageEquation * .25f;
        private float WhirlwindTime => 3 + Math.Min(.1f * Level, 1.5f);

        private bool ShouldEnd => User.IsDead || User.IsKnocked || _passedTime > WhirlwindTime;

        public override string[] Attributes => new[]
        {
            Translations.Get("whirlwind_duration_change", WhirlwindTime.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("whirlwind_damage_change", Damage.ToString("0.0", CultureInfo.InvariantCulture))
        };

        protected override void DoUse()
        {
            _passedTime = 0;
            _trail.Emit = true;
            Casting = true;
            User.IsAttacking = true;
            _rotationY = 0;
            User.Model.Play(_whirlwindAnimation);
        }

        private void Disable()
        {
            _trail.Emit = false;
            Casting = false;
            User.IsAttacking = false;
            User.LeftWeapon.LockWeapon = false;
            User.Model.Reset();
        }

        public override void Update()
        {
            _trail.Update();
            if (!Casting) return;
            if (ShouldEnd) Disable();

            Rotate();
            ManageParticles();
            if (_frameCounter >= .25f)
            {
                DamageNear();
                _frameCounter = 0;
            }

            _passedTime += Time.DeltaTime;
            _frameCounter += Time.DeltaTime;
            _rotationY += Time.DeltaTime * 1000f;
        }

        private void Rotate()
        {
            User.Model.TargetRotation = Vector3.UnitY * _rotationY;
        }

        private void DamageNear()
        {
            for (var i = World.Entities.Count - 1; i > 0; i--)
            {
                if (!User.InAttackRange(World.Entities[i])) continue;

                World.Entities[i].Damage(Damage, User, out var exp);
                User.XP += exp;
            }
        }

        private void ManageParticles()
        {
            var underChunk = World.GetChunkAt(User.Position);
            if (underChunk == null) return;
            World.Particles.VariateUniformly = true;
            World.Particles.Color = World.GetHighestBlockAt((int)User.Position.X, (int)User.Position.Z)
                .GetColor(underChunk.Biome.Colors);
            World.Particles.Position = User.Position - Vector3.UnitY;
            World.Particles.Scale = Vector3.One * .15f;
            World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
            World.Particles.Direction = (-User.Orientation + Vector3.UnitY * 2.75f) * .15f;
            World.Particles.ParticleLifetime = 1;
            World.Particles.GravityEffect = .1f;
            World.Particles.PositionErrorMargin = new Vector3(.75f, .75f, .75f);
            if (World.Particles.Color == Block.GetColor(BlockType.Grass, underChunk.Biome.Colors))
                World.Particles.Color = underChunk.Biome.Colors.GrassColor;
            World.Particles.Emit();
        }
    }
}