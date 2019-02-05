using System;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Warrior
{
    public class Intercept : CappedSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Intercept.png");
        public override string Description => Translations.Get("intercept_desc");
        public override string DisplayName => Translations.Get("intercept_skill");
        private const float BaseDamage = 30f;
        private const float BaseCooldown = 24f;
        private const float CooldownCap = 12f;
        private const float DurationCap = 0.5f;
        private const float BaseDuration = 0.70f;
        private const float BaseManaCost = 60f;
        
        private float Damage => BaseDamage * (base.Level * 0.15f) + BaseDamage;
        public override float MaxCooldown => Math.Max(BaseCooldown - 0.80f * base.Level, CooldownCap);
        private float Duration => Math.Max(BaseDuration - 0.01f * base.Level, DurationCap);
        public override float ManaCost => BaseManaCost;
        protected override int MaxLevel => 25;
        
        private bool _isMoving;
        private readonly Timer _timer;
        private readonly Timer _dmgTimer;
        private readonly Animation _interceptStance;

        public Intercept()
        {
            _timer = new Timer(1);
            _dmgTimer = new Timer(.05f);
            _interceptStance = AnimationLoader.LoadAnimation("Assets/Chr/WarriorIntercept.dae");
        }

        public override void Update()
        {
            base.Update();

            if (!_isMoving) return;
            if (Player.Model.AnimationBlending == null)
            {
                Player.Model.BlendAnimation(_interceptStance);
            }

            if (_dmgTimer.Tick())
            {
                World.Entities.ToList().ForEach(delegate(IEntity Entity)
                {
                    if ((Entity.Position - Player.Position).LengthSquared < 4 * 4 && !Entity.IsKnocked && Player != Entity)
                    {
                        Entity.KnockForSeconds(3f);
                        Entity.Damage(Damage, Player, out var exp);
                        Player.XP += exp;
                    }
                });
            }
            this.EmitParticles();

            if (_timer.Tick())
            {
                _isMoving = false;
                End();
            }
        }

        private void EmitParticles()
        {
            var underChunk = World.GetChunkAt(Player.Position);
            if (underChunk != null)
            {
                World.Particles.Color = Vector4.One;
                World.Particles.VariateUniformly = true;
                World.Particles.Position = Player.Position - Vector3.UnitY + Player.Orientation * 5f;
                World.Particles.Scale = Vector3.One * .5f;
                World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
                World.Particles.Direction = (-Player.Orientation + Vector3.UnitY * 2.75f) * .15f;
                World.Particles.ParticleLifetime = 1f;
                World.Particles.GravityEffect = .1f;
                World.Particles.PositionErrorMargin = new Vector3(Player.Model.Dimensions.Size.X, 2, Player.Model.Dimensions.Size.Z) * .5f;
            }
            World.Particles.Emit();
        }

        private void End()
        {
            Player.Movement.CaptureMovement = true;
            Player.Model.Reset();
        }

        public override bool MeetsRequirements()
        {
            return base.MeetsRequirements() && !_isMoving;
        }

        public override void Use()
        {
            if(_isMoving) return;
            Player.Movement.Orientate();
            Player.Model.BlendAnimation(_interceptStance);
            _isMoving = true;
            _timer.AlertTime = Duration;
            _timer.Reset();
            Player.Movement.CaptureMovement = false;
            Player.Movement.Move(Player.Physics.MoveFormula(Player.View.LookingDirection.Xz.ToVector3().NormalizedFast()) * 1.5f, Duration, false);
        }
    }
}