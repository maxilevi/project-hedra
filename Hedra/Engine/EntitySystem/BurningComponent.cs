/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/01/2017
 * Time: 04:26 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using OpenTK;
using System.Collections;
using System.Runtime.InteropServices;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Particles;

namespace Hedra.Engine.EntitySystem
{
	/// <summary>
	/// Description of BurningComponent.
	/// </summary>
	internal class BurningComponent : EntityComponent
	{
		private readonly float _totalTime;
	    private readonly float _totalDamage;
		private readonly Entity _damager;
	    private float _time;
        private int _pTime;

        public BurningComponent(Entity Parent, Entity Damager, float TotalTime, float TotalDamage) : base(Parent)
        {
			this._totalTime = TotalTime;
			this._totalDamage = TotalDamage;
			this._damager = Damager;
			CoroutineManager.StartCoroutine(UpdateBleed);
		}
		
		public BurningComponent(Entity Parent, float TotalTime, float TotalDamage) : base(Parent)
        {
			this._totalTime = TotalTime;
			this._totalDamage = TotalDamage;
			CoroutineManager.StartCoroutine(UpdateBleed);
		}
		
		public override void Update(){}
		
		public IEnumerator UpdateBleed()
        {
			Parent.Model.BaseTint = Particle3D.FireColor * 3f;
			while(_totalTime > _pTime && !Parent.IsDead && !Disposed && !Parent.IsUnderwater){
				
				_time += Time.DeltaTime;
				if(_time >= 1){
					_pTime++;
					_time = 0;
				    Parent.Damage(_totalDamage / _totalTime, _damager, out float exp);
				    if(_damager is Humanoid damager)
						damager.XP += exp;
				}
				
				//Fire particles
				World.Particles.Color = Particle3D.FireColor;
				World.Particles.VariateUniformly = false;
				World.Particles.Position = Parent.Position + Vector3.UnitY * Parent.Model.Height * .5f;
				World.Particles.Scale = Vector3.One * .5f;
				World.Particles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
				World.Particles.Direction = Vector3.UnitY * .2f;
				World.Particles.ParticleLifetime = 0.75f;
				World.Particles.GravityEffect = 0.0f;
				World.Particles.PositionErrorMargin = Parent.Model.Dimensions.Size * .5f;
				
				for(int i = 0; i < 1; i++){
					World.Particles.Emit();
				}
				yield return null;
			}
			Parent.Model.BaseTint = Vector4.Zero;
			this.Dispose();
			Parent.RemoveComponent(this);
		}
	}
}
