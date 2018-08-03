/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/07/2016
 * Time: 11:37 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Rendering;
using OpenTK;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.Player
{

	public class ParticleProjectile : Projectile
	{
		public ParticleSystem Particles { get; }
	    public bool UseLight { get; set; } = true;
	    public Vector4 Color { get; set; } = Particle3D.FireColor;

        private PointLight _light;
		
		public ParticleProjectile(IEntity Parent, Vector3 Origin) : base(Parent, Origin, new VertexData())
        {		
			Particles = new ParticleSystem();		
		}
		
		public override void Update()
        { 
	        if(Disposed) return;
            base.Update();

		    this.UpdateLighting();

			Particles.Position = this.Position;
			Particles.Color = Color;
			Particles.ParticleLifetime = 1f;
			Particles.GravityEffect = 0f;
			Particles.PositionErrorMargin = new Vector3(1f, 1f, 1f);
			Particles.Scale = Vector3.One * .25f;
			Particles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
            Particles.Emit();

            Particles.Position = this.Position;
            Particles.Color = Color;
            Particles.ParticleLifetime = .05f;
            Particles.GravityEffect = 0f;
            Particles.PositionErrorMargin = new Vector3(1f, 1f, 1f);
            Particles.Scale = Vector3.One * 1.25f;
            Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
            Particles.VariateUniformly = true;
            for(var i = 0; i < 35; i++) Particles.Emit();
        }

	    private void UpdateLighting()
	    {
	        if (UseLight && _light == null)
	        {
	            _light = ShaderManager.GetAvailableLight();
	            if (_light != null)
	                _light.Color = new Vector3(1, 0.2f, 0.2f);
	        }

	        if (_light != null)
	        {
	            _light.Position = this.Position;
	            ShaderManager.UpdateLight(_light);
	        }
        }
		
		public override void Dispose()
        {
			if(_light != null)
            {
				_light.Position = Vector3.Zero;
				_light.Locked = false;
				ShaderManager.UpdateLight(_light);
				_light = null;
			}
            Particles.Dispose();
		    base.Dispose();
		}	
	}
}
