using System.Globalization;
using Hedra.Components.Effects;
using Hedra.Engine.Management;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Particles;
using Hedra.EntitySystem;
using Hedra.Rendering.Particles;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Mage.Druid
{
    public class GroundFissure : SpecialRangedAttackSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/GroundFissure.png");
        private readonly ParticleSystem _particles;
        private readonly Timer _timer;
        private readonly Timer _damageTimer;
        private Vector3 _geyserPosition;
        private bool _isActive;

        public GroundFissure()
        {
            _timer = new Timer(1);
            _damageTimer = new Timer(.1f);
            _particles = new ParticleSystem();
        }

        public override void Update()
        {
            base.Update();
            if (_isActive)
            {
                OutputParticles();
                if (_damageTimer.Tick())
                {
                    DamageEnemies();
                }
            }
            if (_timer.Tick() && _isActive)
            {
                Disable();
            }
        }

        private void CreateExplosion(Vector3 ExplosionPosition)
        {
            World.HighlightArea(ExplosionPosition, Particle3D.FireColor * 2, Radius, Duration + .5f);
            _particles.Color = Particle3D.FireColor;
            _particles.Position = ExplosionPosition;
            _particles.GravityEffect = 0f;
            _particles.Scale = Vector3.One * .75f;
            _particles.ScaleErrorMargin = new Vector3(.5f, .5f, .5f);
            _particles.PositionErrorMargin = new Vector3(2f, 2f, 2f);
            _particles.Shape = ParticleShape.Sphere;       
            _particles.ParticleLifetime = 1.5f + Utils.Rng.NextFloat();
            for(var i = 0; i < 1500; i++)
            {
                var dir = new Vector3(Utils.Rng.NextFloat() * 2 - 1, Utils.Rng.NextFloat(), Utils.Rng.NextFloat() * 2 - 1).NormalizedFast();
                _particles.Direction = dir * (2f + Utils.Rng.NextFloat());
                _particles.Emit();
            }
        }

        private void OutputParticles()
        {
            _particles.Collides = true;
            _particles.Color = Particle3D.FireColor;
            _particles.Position = _geyserPosition;
            _particles.GravityEffect = 0.25f;
            _particles.Scale = Vector3.One * .75f;
            _particles.ScaleErrorMargin = new Vector3(.5f, .5f, .5f);
            _particles.PositionErrorMargin = new Vector3(2f, 2f, 2f);
            _particles.Shape = ParticleShape.Sphere;  
            _particles.ParticleLifetime = 1.5f + Utils.Rng.NextFloat();
            var dir = new Vector3(Utils.Rng.NextFloat() * 2 - 1, Utils.Rng.NextFloat(), Utils.Rng.NextFloat() * 2 - 1).NormalizedFast();
            _particles.Direction = dir * (2f + Utils.Rng.NextFloat());
            //for (var i = 0; i < 5; ++i) _particles.Emit();
                
            _particles.GravityEffect = 0.05f;
            _particles.ParticleLifetime = 1.2f + Utils.Rng.NextFloat();
            _particles.PositionErrorMargin = Vector3.Zero;
            _particles.Shape = ParticleShape.Cone;
            _particles.ConeAngle = 10 * Mathf.Radian;
            _particles.ConeSpeed = 2;
            for(var i = 0; i < 15; ++i) _particles.Emit();
            _particles.Collides = false;
        }

        private void DamageEnemies()
        {
            SkillUtils.DoNearby(User, Radius, (E) =>
            {
                //E.Damage(TotalDamage * _timer.AlertTime / Duration, Player, out var xp);
                //Player.XP += xp;
                if(!E.IsDead && !E.Disposed && E.SearchComponent<BurningComponent>() == null)
                    E.AddComponent(new BurningComponent(E, User, Duration, TotalDamage));
            });
        }
        
        private void Enable(Projectile Proj)
        {
            _geyserPosition = Proj.Position;
            CreateExplosion(Proj.Position);
            _isActive = true;
            _timer.AlertTime = Duration;
            _timer.Reset();
            InvokeStateUpdated();
        }

        private void Disable()
        {
            _isActive = false;
            Cooldown = MaxCooldown;
        }

        protected override int MaxLevel => 15;
        private float Radius => 64 + 48 * (Level / (float) MaxLevel);
        private float Duration => 8 + 8 * (Level / (float) MaxLevel);
        private float TotalDamage => 24 + 32 * (Level / (float) MaxLevel);
        public override float ManaCost => 60;
        public override float MaxCooldown => 33 - 7 * (Level / (float) MaxLevel);
        protected override bool HasCooldown => !_isActive;
        protected override bool ShouldDisable => _isActive;
        public override float IsAffectingModifier => _isActive ? 1 : 0;
        public override string Description => Translations.Get("ground_fissure_desc");
        public override string DisplayName => Translations.Get("ground_fissure_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("ground_fissure_duration_change", Duration.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("ground_fissure_radius_change", Radius.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("ground_fissure_damage_change", (TotalDamage / Duration).ToString("0.0", CultureInfo.InvariantCulture))
        };
        
        protected override void OnHit(Projectile Proj, IEntity Victim)
        {
            base.OnHit(Proj, Victim);
            Enable(Proj);
        }
        
        protected override void OnLand(Projectile Proj)
        {
            base.OnLand(Proj);
            Enable(Proj);
        }
    }
}