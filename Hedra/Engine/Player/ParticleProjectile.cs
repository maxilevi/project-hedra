/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/07/2016
 * Time: 11:37 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Collections;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Rendering.Particles;
using Hedra.EntitySystem;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.WorldObjects;

namespace Hedra.Engine.Player
{
    public class ParticleProjectile : Projectile
    {
        private readonly bool _initialized;

        private PointLight _light;

        public ParticleProjectile(IEntity Parent, Vector3 Origin) : base(Parent, Origin, new VertexData())
        {
            Particles = new ParticleSystem();
            _initialized = true;
        }

        protected ParticleSystem Particles { get; }
        public bool UseLight { get; set; } = true;
        public float LightRadius { get; set; } = 64;
        public Vector4 Color { get; set; } = Particle3D.FireColor;
        
        public float ParticleLifetime { get; set; } = 0.05f;

        public Vector3 Size { get; set; } = Vector3.One;

        public override void Update()
        {
            if (Disposed) return;
            base.Update();

            UpdateLighting();
            DoParticles();
        }

        protected override Box GetCollisionBox(VertexData MeshData)
        {
            return new Box(Vector3.Zero, Size * 1.5f);
        }

        protected virtual void DoParticles()
        {
            Particles.Position = Position;
            Particles.Color = Color;
            Particles.ParticleLifetime = 1f;
            Particles.GravityEffect = 0f;
            Particles.PositionErrorMargin = Size;
            Particles.Scale = Vector3.One * .25f;
            Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
            Particles.Emit();

            Particles.Position = Position;
            Particles.Color = Color;
            Particles.ParticleLifetime = ParticleLifetime;
            Particles.GravityEffect = 0f;
            Particles.PositionErrorMargin = Size;
            Particles.Scale = Vector3.One * 1.25f;
            Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
            Particles.VariateUniformly = true;
            for (var i = 0; i < 35; i++) Particles.Emit();
        }

        private void UpdateLighting()
        {
            if (UseLight && _light == null && !Disposed)
            {
                _light = ShaderManager.GetAvailableLight();
                if (_light != null)
                {
                    _light.Color = Color.Xyz();
                    _light.Radius = LightRadius;
                }
            }

            if (_light != null)
            {
                _light.Position = Position;
                ShaderManager.UpdateLight(_light);
            }
        }

        public override void Dispose()
        {
            if (Disposed) return;
            base.Dispose();
            RoutineManager.StartRoutine(DisposeCoroutine);
        }

        private IEnumerator DisposeCoroutine()
        {
            while (!_initialized) yield return null;
            if (_light != null)
            {
                while (_light.Color.LengthFast() > 0.05f)
                {
                    _light.Color -= _light.Color * Time.DeltaTime * 2f;
                    ShaderManager.UpdateLight(_light);
                    yield return null;
                }

                _light.Position = Vector3.Zero;
                _light.Locked = false;
                ShaderManager.UpdateLight(_light);
                _light = null;
            }

            Particles.Dispose();
        }
    }
}