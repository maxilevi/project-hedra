using System;
using System.Globalization;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Rendering;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.SkillSystem.Warrior
{
    public class Intercept : CappedSkill<IPlayer>
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Intercept.png");
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
        protected override bool ShouldDisable => _isMoving;
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
            if (User.Model.AnimationBlending == null)
            {
                User.Model.BlendAnimation(_interceptStance);
            }

            if (_dmgTimer.Tick())
            {
                World.Entities.ToList().ForEach(delegate(IEntity Entity)
                {
                    if ((Entity.Position - User.Position).LengthSquared() < 4 * 4 && !Entity.IsKnocked && User != Entity)
                    {
                        Entity.KnockForSeconds(3f);
                        Entity.Damage(Damage, User, out var exp);
                        User.XP += exp;
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
            var underChunk = World.GetChunkAt(User.Position);
            if (underChunk != null)
            {
                World.Particles.Color = Vector4.One;
                World.Particles.VariateUniformly = true;
                World.Particles.Position = User.Position - Vector3.UnitY + User.Orientation * 5f;
                World.Particles.Scale = Vector3.One * .5f;
                World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
                World.Particles.Direction = (-User.Orientation + Vector3.UnitY * 2.75f) * .15f;
                World.Particles.ParticleLifetime = 1f;
                World.Particles.GravityEffect = .1f;
                World.Particles.PositionErrorMargin = new Vector3(User.Model.Dimensions.Size.X, 2, User.Model.Dimensions.Size.Z) * .5f;
            }
            World.Particles.Emit();
        }

        private void End()
        {
            User.Movement.CaptureMovement = true;
            User.Model.Reset();
        }

        protected override void DoUse()
        {
            if(_isMoving) return;
            User.Movement.Orientate();
            User.Model.BlendAnimation(_interceptStance);
            _isMoving = true;
            _timer.AlertTime = Duration;
            _timer.Reset();
            User.Movement.CaptureMovement = false;
            User.Movement.Move(User.Physics.MoveFormula(User.View.LookingDirection.Xz().ToVector3().NormalizedFast()) * 1.5f, Duration, false);
        }

        public override string[] Attributes => new[]
        {
            Translations.Get("intercept_damage_change", Damage.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("intercept_duration_change", Duration.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}
