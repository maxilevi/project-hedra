using System;
using Hedra.Engine.Management;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.SkillSystem;
using Hedra.Engine.Sound;
using Hedra.EntitySystem;
using Hedra.Rendering.Particles;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.Player
{
    public class Firewave : ParticleProjectile
    {
        private const int Distance = 32;
        private const int DistanceSquared = Distance * Distance;
        private IEntity[] _ignore;
        private IEntity _owner;
        private float _damage;
        private bool _shouldStop;
        private float _charge;

        private Firewave(IEntity Parent, Vector3 Origin) : base(Parent, Origin)
        {
        }

        private void Explode()
        {
            CreateExplosion();
        }

        public override void Update()
        {
            base.Update();
            if (Disposed) return;
            if (_shouldStop && Particles.ParticleCount == 0) Dispose();
        }

        private void CreateExplosion()
        {
            CreateParticles();
            DamageEntities();
            SoundPlayer.PlaySound(SoundType.HitGround, Position);
            _shouldStop = true;
            SkillUtils.DoNearby(_owner, Distance, Entity =>
            {
                var direction = (Entity.Position - _owner.Position).Xz.ToVector3().NormalizedFast();
                Entity.Physics.ApplyImpulse(direction * 96 * _charge * 2f);
            });
        }
        
        private void DamageEntities()
        {
            var entities = World.Entities;
            for (var i = 0; i < entities.Count; i++)
            {
                if (entities[i] == _owner || Array.IndexOf(_ignore, entities[i]) != -1) continue;
                if ((entities[i].Position - _owner.Position).LengthSquared < DistanceSquared)
                {
                    World.Entities[i].Damage(_damage, _owner, out var exp);
                    if (_owner is IHumanoid humanoid)
                        humanoid.XP += exp;
                }
            }
        }

        private void CreateParticles()
        {
            Particles.Color = Particle3D.FireColor;
            Particles.Position = Position;
            Particles.GravityEffect = 0f;
            Particles.Scale = Vector3.One * .5f;
            Particles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
            Particles.PositionErrorMargin = new Vector3(2f,2f,2f);
            Particles.Shape = ParticleShape.Sphere;       
            Particles.ParticleLifetime = Math.Max(2.0f * _charge, .25f);
            
            for(var i = 0; i < 750; i++)
            {
                var dir = new Vector3(Utils.Rng.NextFloat() * 2 - 1, Utils.Rng.NextFloat(), Utils.Rng.NextFloat() * 2 - 1);
                Particles.Direction = dir * .75f;
                Particles.Emit();
            }
        }

        /* This method is empty so that the base class doesn't output particles.*/
        protected override void DoParticles()
        {
        }

        public static void Create(IEntity Owner, float Damage, float Charge = 1, params IEntity[] Ignore)
        {
            var release = new Firewave(Owner, Owner.Position)
            {
                Color = Particle3D.FireColor,
                Direction = Vector3.Zero,
                UsePhysics = false,
                HandleLifecycle = false,
                UseLight = true,
                Speed = 0,
                _owner = Owner,
                _damage = Damage,
                _charge = Charge,
                _ignore = Ignore
            };
            release.Explode();
        }
    }
}
