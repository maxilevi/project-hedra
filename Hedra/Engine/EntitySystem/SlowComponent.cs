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
using Hedra.Engine.Management;

namespace Hedra.Engine.EntitySystem
{
    /// <inheritdoc cref="IEffectComponent" />
    /// <summary>
    /// Description of FireComponent.
    /// </summary>
    public class SlowComponent : EntityComponent, IEffectComponent
    {
        public int Chance { get; set; } = 25;
        public float TotalStrength { get; set; } = 60;
        public float BaseTime { get; set; } = 5;

	    private float _cooldown;
		private bool _canSlow = true;
		
		public SlowComponent(Entity Parent) : base(Parent)
		{
            Parent.OnAttacking += this.Apply;
		}
		
		public override void Update(){
			_cooldown -= Time.FrameTimeSeconds;
		}
		
		public void Apply(Entity Victim, float Amount){
			if(_cooldown > 0 || !_canSlow) return;
			
			bool shouldSlow = Utils.Rng.NextFloat() <= Chance * 0.01;
			if(shouldSlow){
				_slowTime =  5 + Utils.Rng.NextFloat() * 4 -2f;
				_pTime = 0;
				_oldSpeed = Victim.Speed;
				Victim.Speed *= (TotalStrength/100);
				this._victim = Victim;
				
				CoroutineManager.StartCoroutine(SlowCoroutine);
			}
		}
		
		private float _slowTime = 0;
		private float _pTime = 0;
		private float _oldSpeed = 0;
		private Entity _victim;
		public IEnumerator SlowCoroutine(){
			_victim.Model.BaseTint = new Vector4(2,2,1,1) * .7f;
			this._canSlow = false;
			while(_slowTime > _pTime && !_victim.IsDead){
				_pTime += Time.FrameTimeSeconds;
				yield return null;
			}
			_victim.Speed = _oldSpeed; 
			_victim.Model.BaseTint = Vector4.Zero;
			this._canSlow = true;
			this._cooldown = 4;
		}
	}
}
