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
using Hedra.Engine.Management;
using Hedra.Engine.Player;

namespace Hedra.Engine.EntitySystem
{
    /// <inheritdoc cref="IEffectComponent" />
	/// <summary>
	/// Description of FireComponent.
	/// </summary>
	public class FreezeComponent : EntityComponent, IEffectComponent
	{
	    public int Chance { get; set; } = 20; //20%
	    public float TotalStrength { get; set; } = 30;
		public float BaseTime { get; set; } = 5;
		
		private float _cooldown;
		private bool _canFreeze = true;
		private float _oldSpeed = 0;
		private float _freezeTime = 0;
		private float _time = 0;
		private float _pTime = 0;
		private Entity _victim;
		
		public FreezeComponent(Entity Parent) : base(Parent) {
			Parent.OnAttacking += new OnAttackEventHandler(this.Apply);
		}
		
		public override void Update(){
			_cooldown -= Engine.Time.FrameTimeSeconds;
		}
		
		public void Apply(Entity Victim, float Amount){
			if(_cooldown > 0 || !_canFreeze) return;
			
			bool shouldFreeze = Utils.Rng.NextFloat() <= Chance * 0.01 ? true : false;
			if(shouldFreeze){
				_freezeTime =  5 + Utils.Rng.NextFloat() * 4 -2f;
				_time = 0;
				_pTime = 0;
				_oldSpeed = Victim.Speed;
				Victim.Speed = 0;
				Victim.Model.Pause = true;
				this._victim = Victim;
				
				CoroutineManager.StartCoroutine(FreezeCoroutine);
			}
		}
		
		public IEnumerator FreezeCoroutine(){
			_victim.Model.BaseTint = Bar.Blue * new Vector4(1,1,2,1) * .7f;
			this._canFreeze = false;
			while(_freezeTime > _pTime && !_victim.IsDead && !Disposed){
				
				_time += Engine.Time.FrameTimeSeconds;
				if(_time >= 1){
					_pTime++;
					_time = 0;
					float exp;
					_victim.Damage(TotalStrength / _freezeTime, this.Parent, out exp, true);
					if(Parent is LocalPlayer)
						(Parent as LocalPlayer).XP += exp;
				}
				yield return null;
			}
			_victim.Model.Pause = false;
			_victim.Speed = _oldSpeed;
			_victim.Model.BaseTint = Vector4.Zero;
			this._canFreeze = true;
			this._cooldown = 4;
		}
	}
}
