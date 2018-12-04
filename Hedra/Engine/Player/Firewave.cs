using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.Sound;
using Hedra.EntitySystem;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.Player
{
    public class Firewave : ParticleProjectile
    {
        private const int DistanceSquared = 32 * 32;
        private IHumanoid _owner;
        private float _damage;
        private bool _shouldStop;
        private float _charge;

        private Firewave(IEntity Parent, Vector3 Origin) : base(Parent, Origin)
        {
        }

        public void Explode()
        {
            CreateExplosion();
        }

        public override void Update()
        {
            base.Update();
            if (Disposed) return;
            if (_shouldStop && Particles.Particles.Count == 0) Dispose();
            PushEntitiesAway();
        }

        private void CreateExplosion()
        {
            CreateParticles();
            DamageEntities();
            SoundPlayer.PlaySound(SoundType.HitGround, Position);
            _shouldStop = true;
        }
        
        private void DamageEntities()
        {
            var entities = World.Entities;
            for (var i = 0; i < entities.Count; i++)
            {
                if (entities[i] == _owner) continue;
                if ((entities[i].Position - _owner.Position).LengthSquared < DistanceSquared)
                {
                    World.Entities[i].Damage(_damage, _owner, out var exp);
                    _owner.XP += exp;
                }
            }
        }
        
        public void PushEntitiesAway()
        {
            var entities = World.Entities;
            for(var i = 0; i< entities.Count; i++)
            {
                if((_owner.Position - entities[i].Position).LengthSquared < DistanceSquared * _charge)
                {
                    if(_owner == entities[i]) continue;
                    
                    var direction = -(_owner.Position - entities[i].Position).NormalizedFast();
                    entities[i].Physics.DeltaTranslate(direction * 128 * _charge);
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

        public static void Create(IHumanoid Owner, float Damage, float Charge = 1)
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
                _charge = Charge
            };
            release.Explode();
        }
    }
}
