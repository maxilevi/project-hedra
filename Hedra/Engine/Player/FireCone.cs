using System;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Rendering.Particles;
using Hedra.EntitySystem;
using Hedra.Numerics;
using Hedra.Rendering.Particles;

namespace Hedra.Engine.Player
{
    public class FireCone : ParticleProjectile
    {
        private const int ConeDistanceSquared = 320;
        private float _damagePerSecond;
        private Action _do;
        private IHumanoid _owner;
        private bool _shouldStop;
        private Func<bool> _while;

        private FireCone(IEntity Parent, Vector3 Origin) : base(Parent, Origin)
        {
        }

        public override void Update()
        {
            base.Update();
            if (Disposed) return;
            if (_shouldStop && Particles.ParticleCount == 0) Dispose();
            if (!Disposed && !_shouldStop)
                UpdateDamage();
            _do();
            if (!_while()) _shouldStop = true;
        }

        protected override void DoParticles()
        {
            if (_shouldStop) return;
            Particles.Position = (_owner.Model.RightWeaponPosition + _owner.Model.LeftWeaponPosition) * .5f;
            Particles.Direction = _owner.Orientation;
            Particles.Color = new Vector4(1, .3f, 0, 1);
            Particles.ParticleLifetime = 2.25f;
            Particles.GravityEffect = 0f;
            Particles.Scale = Vector3.One * .5f;
            Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
            Particles.VariateUniformly = false;
            Particles.Shape = ParticleShape.Cone;
            Particles.ConeAngle = -95f;
            for (var i = 0; i < 15; i++) Particles.Emit();
        }

        private void UpdateDamage()
        {
            var entities = World.Entities;
            for (var i = 0; i < entities.Count; i++)
            {
                if (entities[i] == _owner) continue;

                var distanceVector = entities[i].Position - _owner.Position;
                var toEntity = distanceVector.NormalizedFast();
                var dot = Vector3.Dot(toEntity, _owner.Orientation);

                if (dot >= .75f && distanceVector.LengthSquared() < ConeDistanceSquared)
                {
                    var k = i;
                    var mod = 1f - distanceVector.LengthSquared() / ConeDistanceSquared;
                    _owner.DoIgnoringHitCombo(() =>
                    {
                        World.Entities[k].Damage(_damagePerSecond * dot * Time.DeltaTime * mod, _owner, out var exp,
                            false,
                            false);
                        _owner.XP += exp;
                    });
                }
            }
        }

        public static void Create(IHumanoid Owner, float DamagePerSecond, float Charge)
        {
            var k = Charge;
            Create(Owner, DamagePerSecond, () => k > 0, () => k -= Time.DeltaTime);
        }

        public static FireCone Create(IHumanoid Owner, float DamagePerSecond, Func<bool> While, Action Do)
        {
            return new FireCone(Owner, Owner.Position)
            {
                Color = Particle3D.FireColor,
                Direction = Vector3.Zero,
                UsePhysics = false,
                HandleLifecycle = false,
                UseLight = true,
                Speed = 0,
                Collide = false,
                _do = Do,
                _while = While,
                _owner = Owner,
                _damagePerSecond = DamagePerSecond
            };
        }
    }
}