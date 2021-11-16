/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/01/2017
 * Time: 04:26 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Collections;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.EntitySystem;
using Hedra.Rendering;

namespace Hedra.Components.Effects
{
    /// <summary>
    ///     Description of BurningComponent.
    /// </summary>
    public class BleedingComponent : DamagingEffectComponent
    {
        private int _pTime;
        private float _time;

        public BleedingComponent(IEntity Parent, IEntity Damager, float TotalTime, float TotalDamage) : base(Parent,
            TotalTime, TotalDamage, Damager)
        {
            RoutineManager.StartRoutine(UpdateBleed);
        }

        public BleedingComponent(IEntity Parent, float TotalTime, float TotalDamage) : this(Parent, null, TotalTime,
            TotalDamage)
        {
        }

        public override DamageType DamageType => DamageType.Bleed;

        public override void Update()
        {
        }

        public IEnumerator UpdateBleed()
        {
            Parent.Model.BaseTint = Colors.LowHealthRed * new Vector4(3, 1, 1, 1) * .7f;
            while (TotalTime > _pTime && !Parent.IsDead && !Disposed)
            {
                _time += Time.DeltaTime;
                if (_time >= 1)
                {
                    _pTime++;
                    _time = 0;
                    Damage();
                }

                //Fire particles
                World.Particles.Color = new Vector4(.8f, .0f, 0, 1f);
                World.Particles.VariateUniformly = false;
                World.Particles.Position = Parent.Position + Vector3.UnitY * Parent.Model.Height * .5f;
                World.Particles.Scale = Vector3.One * .5f;
                World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
                World.Particles.Direction = Vector3.UnitY * .2f;
                World.Particles.ParticleLifetime = 0.75f;
                World.Particles.GravityEffect = 0.5f;
                World.Particles.PositionErrorMargin = Parent.Model.Dimensions.Size * .5f;

                for (var i = 0; i < 1; i++) World.Particles.Emit();
                yield return null;
            }

            Parent.Model.BaseTint = Vector4.Zero;
            Dispose();
            Parent.RemoveComponent(this);
        }
    }
}