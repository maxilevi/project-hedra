using System;
using System.Globalization;
using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.Particles;
using Hedra.EntitySystem;
using Hedra.Rendering.Particles;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Mage
{
    public class Inferno : SingleAnimationSkill<IPlayer>
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skill/Inferno.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/MageInferno.dae");
        protected override float AnimationSpeed => 1.75f;

        protected override void OnAnimationEnd()
        {
            base.OnAnimationEnd();
            InfernoFireball.Create(
                User,
                User.Position + Vector3.UnitY * 4f,
                User.LookingDirection,
                Radius,
                Damage
            );
        }

        private float Damage => 40 + 55 * (Level /(float) MaxLevel);
        private float Radius => 16 + 32 * (Level /(float) MaxLevel);
        protected override int MaxLevel => 20;
        public override float MaxCooldown => 54;
        public override float ManaCost => 110;
        public override string Description => Translations.Get("inferno_desc");
        public override string DisplayName => Translations.Get("inferno_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("inferno_damage_change", Damage.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("inferno_radius_change", Radius.ToString("0.0", CultureInfo.InvariantCulture))
        };

        private class InfernoFireball : ParticleProjectile
        {
            private readonly ParticleSystem _particles;
            private IHumanoid _parent;
            private float _radius;
            private float _damage;
            
            private InfernoFireball(IHumanoid Parent, Vector3 Origin) : base(Parent, Origin)
            {
                _particles = new ParticleSystem();
            }
            
            protected override void DoParticles()
            {
                _particles.Position = Position;
                _particles.Color = new Vector4(1, .3f, 0, 1);
                _particles.Shape = ParticleShape.Sphere;
                _particles.ParticleLifetime = .3f;
                _particles.GravityEffect = 0f;
                _particles.PositionErrorMargin = new Vector3(5f, 5f, 5f);
                _particles.Scale = Vector3.One * .75f;
                _particles.ScaleErrorMargin = new Vector3(.4f, .4f, .4f);
                _particles.VariateUniformly = false;
                for (var i = 0; i < 50; i++) _particles.Emit();
            }
            
            public static void Create(IHumanoid Owner, Vector3 Position, Vector3 Direction, float Radius, float Damage)
            {
                var fireball = new InfernoFireball(Owner, Position)
                {
                    Direction = Direction,
                    UsePhysics = false,
                    UseLight = true,
                    Speed = 1.5f,
                    _parent = Owner,
                    _radius = Radius,
                    _damage = Damage
                };
                fireball.HitEventHandler += (_, __) =>
                {
                    fireball.Explode();
                };
                fireball.LandEventHandler += _ =>
                {
                    fireball.Explode();
                };
                World.AddWorldObject(fireball);
            }

            private void Explode()
            {
                SkillUtils.DoNearby(_parent, Position, _radius, (E) =>
                {
                    E.Damage(_damage, _parent, out var xp);
                    _parent.XP += xp;
                    if (Utils.Rng.Next(0, 5) == 1)
                    {
                        E.AddComponent(new BurningComponent(E, _parent, 4, _damage));
                    }
                });

                StartExplodeParticles();
                TaskScheduler.For(.5f, ExplodeParticles);
                SoundPlayer.PlaySound(SoundType.GroundQuake, Position);
            }

            private void ExplodeParticles()
            {
                _particles.Collides = true;
                _particles.Color = Particle3D.FireColor;
                _particles.Position = Position;
                _particles.GravityEffect = 0f;
                _particles.Scale = Vector3.One * .75f;
                _particles.ScaleErrorMargin = new Vector3(.5f, .5f, .5f);
                _particles.PositionErrorMargin = new Vector3(2f, 2f, 2f);
                _particles.Shape = ParticleShape.Sphere;       
                _particles.ParticleLifetime = 1.5f + Utils.Rng.NextFloat();
                var dir = new Vector3(Utils.Rng.NextFloat() * 2 - 1, Utils.Rng.NextFloat(), Utils.Rng.NextFloat() * 2 - 1).NormalizedFast();
                _particles.Direction = dir * (2f + Utils.Rng.NextFloat());
                for (var i = 0; i < 5; ++i) _particles.Emit();
                
                _particles.GravityEffect = .25f;
                _particles.ParticleLifetime = 2f + Utils.Rng.NextFloat();
                _particles.PositionErrorMargin = new Vector3(_radius, 2f, _radius) * 2;
                _particles.Direction = Vector3.UnitY * 4;
                for(var i = 0; i < 5; ++i) _particles.Emit();
                _particles.Collides = false;
            }

            private void StartExplodeParticles()
            {
                World.HighlightArea(Position, Particle3D.FireColor, _radius, 2);
                _particles.Color = Particle3D.FireColor;
                _particles.Position = Position;
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

            public override void Dispose()
            {
                base.Dispose();
                TaskScheduler.After(8, () => _particles.Dispose());
            }
        }
    }
}