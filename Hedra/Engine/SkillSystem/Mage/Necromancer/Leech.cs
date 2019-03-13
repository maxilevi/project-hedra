using System;
using System.Drawing;
using System.Globalization;
using Hedra.Engine.Management;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;
using Hedra.Rendering.Particles;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Mage.Necromancer
{
    public class Leech : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Leech.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/NecromancerTerror.dae");
        protected override bool CanMoveWhileCasting => false;
        protected override float AnimationSpeed => .75f;

        protected override void OnAnimationEnd()
        {
            base.OnAnimationEnd();
            World.HighlightArea(Player.Position, Colors.Red * .5f, Radius * 2.0f, .5f);
            SpawnParticles();
            SoundPlayer.PlaySound(SoundType.GroundQuake, Player.Position);
            SkillUtils.DoNearby(Player, Radius, E =>
            {
                if (E.SearchComponent<DamageComponent>().HasIgnoreFor(Player)) return;
                E.AddComponentForSeconds(new LeechComponent(E, Player, Damage, Health), Duration);
            });
        }
        
        private void SpawnParticles()
        {
            World.Particles.Color = Colors.Red * .5f;
            World.Particles.VariateUniformly = true;
            World.Particles.Position = Player.Position;
            World.Particles.GravityEffect = 0f;
            World.Particles.Scale = Vector3.One * .5f;
            World.Particles.ScaleErrorMargin = Vector3.One * .35f;
            World.Particles.PositionErrorMargin = Vector3.One * 2;
            World.Particles.Shape = ParticleShape.Sphere;       
            World.Particles.ParticleLifetime = .05f * Radius;       
            for(var i = 0; i < 500; ++i)
            {
                var dir = new Vector3(Utils.Rng.NextFloat() * 2 - 1, 0f, Utils.Rng.NextFloat() * 2 - 1).NormalizedFast();
                World.Particles.Direction = dir;
                World.Particles.Emit();
            }
        }

        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("leech_desc");
        public override string DisplayName => Translations.Get("leech_skill");
        private float Radius => 48 + 48 * (Level / (float) MaxLevel);
        private float Health => Damage;
        private float Damage => Math.Min(10, 5 + 5 * (Level / 10));
        private float Duration => 7 + 10 * (Level / (float)MaxLevel);
        public override float ManaCost => 60;
        public override float MaxCooldown => 27 - 5 * (MaxLevel / (float) MaxLevel) + Duration;
        public override string[] Attributes => new[]
        {
            Translations.Get("leech_damage_and_health_change", Damage.ToString("0.0", CultureInfo.InvariantCulture), Health.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("leech_duration_change", Duration.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("leech_radius_change", Radius.ToString("0.0", CultureInfo.InvariantCulture))
        };

        private class LeechComponent : EntityComponent
        {
            private readonly IPlayer _caster;
            private readonly Timer _timer;
            private readonly float _damagePerSecond;
            private readonly float _healPerSecond;
            
            public LeechComponent(IEntity Entity, IPlayer Caster, float DamagePerSecond, float HealthPerSecond) : base(Entity)
            {
                _caster = Caster;
                _timer = new Timer(1 + Utils.Rng.NextFloat());
                _damagePerSecond = DamagePerSecond;
                _healPerSecond = HealthPerSecond;
                Parent.Model.Outline = true;
                Parent.Model.OutlineColor = new Vector4(Colors.Red.Xyz * .75f, 1);
            }

            public override void Update()
            {
                if (_timer.Tick())
                {
                    Parent.Damage(_damagePerSecond, _caster, out var xp);
                    _caster.XP += xp;
                    BloodSkill.LaunchParticle(_caster, Parent, _caster, (_, __) =>
                    {
                        _caster.ShowText($"+{_healPerSecond} HP", Color.LawnGreen, 14);
                        _caster.Health += _healPerSecond;
                        _caster.Model.Outline = true;
                        _caster.Model.OutlineColor = Colors.GreenYellow;
                        TaskScheduler.After(.5f, () =>
                        {
                            _caster.Model.Outline = false;
                        });
                    });
                }
            }

            public override void Dispose()
            {
                base.Dispose();
                Parent.Model.Outline = false;
            }
        }
    }
}