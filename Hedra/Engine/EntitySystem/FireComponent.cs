/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 14/12/2016
 * Time: 10:25 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
 using OpenTK;
using System.Collections;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Player;
using Hedra.Engine.Generation;
using Hedra.Engine.Rendering.Particles;


namespace Hedra.Engine.EntitySystem
{
    /// <inheritdoc cref="IEffectComponent" />
    /// <summary>
    /// Description of FireComponent.
    /// </summary>
    public class FireComponent : EntityComponent, IEffectComponent
    {
		public int Chance { get; set; } = 20;//20%
		public float TotalStrength { get; set; } = 30;
		public float BaseTime { get; set; } = 5;
		
		private float _cooldown;
		private bool _canFire = true;
		
		public FireComponent(Entity Parent) : base(Parent) {
			Parent.OnAttacking += new OnAttackEventHandler(this.Apply);
		}
		
		public override void Update(){
			_cooldown -= Engine.Time.FrameTimeSeconds;
		}
		
		public void Apply(Entity Victim, float Amount){
			if(_cooldown > 0 || !_canFire) return;
			
			bool shouldFire = Utils.Rng.NextFloat() <= Chance * 0.01 ? true : false;
			if(shouldFire){
				_fireTime =  BaseTime + Utils.Rng.NextFloat() * 4 -2f;
				if(Victim.SearchComponent<BurningComponent>() == null){
					Victim.AddComponent(new BurningComponent(Victim, Parent, _fireTime, TotalStrength));
				}
			}
		}
		
		private float _fireTime = 0;
		private float _time = 0;
		private float _pTime = 0;
		private Entity _victim;
		public IEnumerator FireCoroutine(){
			_victim.Model.BaseTint = Bar.Low * new Vector4(1,3,1,1) * .7f;
			this._canFire = false;
			while(_fireTime > _pTime && !_victim.IsDead && !Disposed){
				
				_time += Engine.Time.FrameTimeSeconds;
				if(_time >= 1){
					_pTime++;
					_time = 0;
					float exp;
					_victim.Damage(TotalStrength / _fireTime, this.Parent, out exp, true);
					if(Parent is LocalPlayer)
						(Parent as LocalPlayer).XP += exp;
				}
				
				//Fire particles
				World.WorldParticles.Color = Particle3D.FireColor;
				World.WorldParticles.VariateUniformly = false;
				World.WorldParticles.Position = _victim.Position;
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
			_victim.Model.BaseTint = Vector4.Zero;
			this._canFire = true;
			this._cooldown = 4;
		}
	}
}
