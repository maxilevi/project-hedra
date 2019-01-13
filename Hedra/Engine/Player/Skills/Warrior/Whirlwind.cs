/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:13 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using Hedra.Core;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player.ToolbarSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.Particles;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.Player.Skills.Warrior
{
    /// <summary>
    /// Description of WeaponThrow.
    /// </summary>
    public class Whirlwind : CappedSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Spin.png");
        protected override bool Grayscale => !Player.HasWeapon;
        protected override int MaxLevel => 24;
        public override string Description => Translations.Get("whirlwind_desc");        
        public override string DisplayName => Translations.Get("whirlwind");
        public override float ManaCost => Math.Max(120 - 4f * base.Level, 40);
        public override float MaxCooldown => (float) Math.Max(12.0 - .25f * base.Level, 6) + WhirlwindTime;
        private float Damage => Player.DamageEquation * .25f;
        private float WhirlwindTime => 3 + Math.Min(.1f * base.Level, 1.5f);

        private readonly Animation _whirlwindAnimation;
        private readonly TrailRenderer _trail;
        private float _frameCounter;
        private float _passedTime;
        private float _rotationY;

        public Whirlwind() 
        {
            _trail = new TrailRenderer( () => Player.LeftWeapon.WeaponTip, Vector4.One);
            _whirlwindAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorWhirlwind.dae");
            _whirlwindAnimation.OnAnimationEnd += delegate
            {
                if (!Casting) return;
                Player.Model.Play(_whirlwindAnimation);
            };
            _whirlwindAnimation.Loop = false;
        }
        
        public override void Use()
        {
            _passedTime = 0;
            _trail.Emit = true;
            Casting = true;
            Player.IsAttacking = true;
            _rotationY = 0;
            Player.Model.Play(_whirlwindAnimation);
        }

        private void Disable()
        {
            _trail.Emit = false;
            Casting = false;
            Player.IsAttacking = false;
            Player.LeftWeapon.LockWeapon = false;
            Player.Model.Reset();
        }
        
        public override void Update()
        {
            if (!Casting) return;
            if (ShouldEnd) Disable();

            Rotate();
            ManageParticles();            
            if(_frameCounter >= .25f)
            {
                DamageNear();
                _frameCounter = 0;
            }
            _trail.Update();
            _passedTime += Time.DeltaTime;
            _frameCounter += Time.DeltaTime;
            _rotationY += Time.DeltaTime * 1000f;
        }

        private bool ShouldEnd => Player.IsDead || Player.IsKnocked || _passedTime > WhirlwindTime;

        private void Rotate()
        {
            Player.Model.TargetRotation = Vector3.UnitY * _rotationY;
        }
        
        private void DamageNear()
        {
            for (var i = World.Entities.Count - 1; i > 0; i--)
            {
                if (!Player.InAttackRange(World.Entities[i])) continue;

                World.Entities[i].Damage(Damage, Player, out var exp);
                Player.XP += exp;
            }
        }
        
        private void ManageParticles()
        {
            var underChunk = World.GetChunkAt(Player.Position);
            World.Particles.VariateUniformly = true;
            World.Particles.Color = World.GetHighestBlockAt((int)this.Player.Position.X, (int)this.Player.Position.Z).GetColor(underChunk.Biome.Colors);
            World.Particles.Position = this.Player.Position - Vector3.UnitY;
            World.Particles.Scale = Vector3.One * .15f;
            World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
            World.Particles.Direction = (-this.Player.Orientation + Vector3.UnitY * 2.75f) * .15f;
            World.Particles.ParticleLifetime = 1;
            World.Particles.GravityEffect = .1f;
            World.Particles.PositionErrorMargin = new Vector3(.75f, .75f, .75f);
            if (World.Particles.Color == Block.GetColor(BlockType.Grass, underChunk.Biome.Colors))
                World.Particles.Color = underChunk.Biome.Colors.GrassColor;
            World.Particles.Emit();
        }
        
        public override bool MeetsRequirements()
        {
            return base.MeetsRequirements() && !Player.Toolbar.DisableAttack && Player.HasWeapon;
        }
    }
}