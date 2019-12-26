/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/01/2017
 * Time: 04:26 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Particles;
using Hedra.EntitySystem;
using System.Numerics;

namespace Hedra.Components.Effects
{
    /// <summary>
    /// Description of BurningComponent.
    /// </summary>
    public class BurningComponent : DamagingEffectComponent
    {
        private int _particleCounter;
        private float _time;
        private int _pTime;

        public override DamageType DamageType => DamageType.Fire;

        public BurningComponent(IEntity Parent, IEntity Damager, float TotalTime, float TotalDamage) : base(Parent, TotalTime, TotalDamage, Damager)
        {
            var weakness = Parent.SearchComponent<FireWeaknessComponent>();
            this.TotalDamage = (TotalDamage * Damager?.Attributes.FireDamageMultiplier ?? 1) * weakness?.Power ?? 1;
            Start();
        }
        
        public BurningComponent(IEntity Parent, float TotalTime, float TotalDamage) : this(Parent, null, TotalTime, TotalDamage)
        {
        }

        private void Start()
        {
            var freeze = Parent.SearchComponent<FreezingComponent>();
            if (freeze != null)
            {
                Parent.RemoveComponent(freeze);
                Parent.RemoveComponent(this);
            }
            else
            {
                Parent.Model.BaseTint = Particle3D.FireColor * 3f;
            }
        }
        
        public override void Update()
        {
            _time += Time.DeltaTime;
            if(_time >= 1)
            {
                _pTime++;
                _time = 0;
                Damage();
            }

            if (_particleCounter % 2 == 0)
            {
                World.Particles.Color = Particle3D.FireColor;
                World.Particles.VariateUniformly = false;
                World.Particles.Position = Parent.Position + Vector3.UnitY * Parent.Model.Height * .5f;
                World.Particles.Scale = Vector3.One * .5f;
                World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
                World.Particles.Direction = Vector3.UnitY * .2f;
                World.Particles.ParticleLifetime = 0.75f;
                World.Particles.GravityEffect = 0.0f;
                World.Particles.PositionErrorMargin = Parent.Model.Dimensions.Size * .5f;
                World.Particles.Emit();
            }
            _particleCounter++;

            if(!(TotalTime > _pTime && !Parent.IsDead && !Disposed && !Parent.IsUnderwater))
                Parent.RemoveComponent(this);
        }

        public override void Dispose()
        {
            base.Dispose();
            Parent.Model.BaseTint = Vector4.Zero;
        }
    }
}
