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
using Hedra.Engine.Management;
using Hedra.Engine.Player;

namespace Hedra.Engine.EntitySystem
{
	/// <summary>
	/// Description of BurningComponent.
	/// </summary>
	public class FreezingComponent : EntityComponent
	{
	    private readonly float _totalTime;
	    private float _time;
	    private readonly float _totalDamage;
		private int _pTime;
		private readonly Entity _damager;
		
		public FreezingComponent(Entity Parent, Entity Damager, float TotalTime, float TotalDamage) : base(Parent){
			this._totalTime = TotalTime;
			this._totalDamage = TotalDamage;
			this._damager = Damager;
			CoroutineManager.StartCoroutine(UpdateFreeze);
		}
		
		public FreezingComponent(Entity Parent, float TotalTime, float TotalDamage) : base(Parent){
			this._totalTime = TotalTime;
			this._totalDamage = TotalDamage;
			CoroutineManager.StartCoroutine(UpdateFreeze);
		}
		
		public override void Update(){}
		
		public IEnumerator UpdateFreeze(){
			Parent.Model.BaseTint = Bar.Blue * new Vector4(1,1,2,1) * .7f;
            Parent.ComponentManager.AddComponentWhile(new SpeedBonusComponent(Parent, -Parent.Speed),
                () => _totalTime > _pTime && !Parent.IsDead && !Disposed);
			Parent.Model.Pause = true;
			while(_totalTime > _pTime && !Parent.IsDead && !Disposed){
				
				_time += Engine.Time.ScaledFrameTimeSeconds;
				if(_time >= 1){
					_pTime++;
					_time = 0;
					float Exp;
					Parent.Damage( (float) (_totalDamage / _totalTime), _damager, out Exp, true);
					if(_damager != null && _damager is Humanoid)
						(_damager as Humanoid).XP += Exp;
				}
				
				yield return null;
			}
			Parent.Model.BaseTint = Vector4.Zero;
			Parent.Model.Pause = false;
			this.Dispose();
			Parent.RemoveComponent(this);
		}
	}
}
