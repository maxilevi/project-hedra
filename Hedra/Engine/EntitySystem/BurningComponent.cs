/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/01/2017
 * Time: 04:26 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.Rendering.UI;
using System.Collections;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Particles;

namespace Hedra.Engine.EntitySystem
{
	/// <summary>
	/// Description of BurningComponent.
	/// </summary>
	public class BurningComponent : EntityComponent
	{
		private readonly float _totalTime;
	    private readonly float _totalDamage;
		private readonly Entity _damager;
	    private float _time;
        private int _pTime;

        public BurningComponent(Entity Parent, Entity Damager, float TotalTime, float TotalDamage) : base(Parent){
			this._totalTime = TotalTime;
			this._totalDamage = TotalDamage;
			this._damager = Damager;
			CoroutineManager.StartCoroutine(UpdateBleed);
		}
		
		public BurningComponent(Entity Parent, float TotalTime, float TotalDamage) : base(Parent){
			this._totalTime = TotalTime;
			this._totalDamage = TotalDamage;
			CoroutineManager.StartCoroutine(UpdateBleed);
		}
		
		public override void Update(){}
		
		public IEnumerator UpdateBleed(){
			Parent.Model.BaseTint = Particle3D.FireColor * 3f;
			while(_totalTime > _pTime && !Parent.IsDead && !Disposed && !Parent.IsUnderwater){
				
				_time += Time.ScaledFrameTimeSeconds;
				if(_time >= 1){
					_pTime++;
					_time = 0;
					float exp;
					Parent.Damage(_totalDamage / _totalTime, _damager, out exp);
				    var damager = _damager as Humanoid;
				    if(damager != null)
						damager.XP += exp;
				}
				
				//Fire particles
				World.WorldParticles.Color = Particle3D.FireColor;
				World.WorldParticles.VariateUniformly = false;
				World.WorldParticles.Position = Parent.Position + Vector3.UnitY * (Parent.HitBox.Max.Y - Parent.HitBox.Min.Y) * .5f;
				World.WorldParticles.Scale = Vector3.One * .5f;
				World.WorldParticles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
				World.WorldParticles.Direction = Vector3.UnitY * .2f;
				World.WorldParticles.ParticleLifetime = 0.75f;
				World.WorldParticles.GravityEffect = 0.0f;
				World.WorldParticles.PositionErrorMargin = new Vector3(1.5f, 1.5f, 1.5f);
				
				for(int i = 0; i < 1; i++){
					World.WorldParticles.Emit();
				}
				yield return null;
			}
			Parent.Model.BaseTint = Vector4.Zero;
			this.Dispose();
			Parent.RemoveComponent(this);
		}
	}
}
