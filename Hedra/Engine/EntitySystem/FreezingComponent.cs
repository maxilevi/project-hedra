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
		private float TotalTime, Time, TotalDamage, OldSpeed;
		private int PTime;
		private Entity Damager = null;
		
		public FreezingComponent(Entity Parent, Entity Damager, float TotalTime, float TotalDamage) : base(Parent){
			this.TotalTime = TotalTime;
			this.TotalDamage = TotalDamage;
			this.Damager = Damager;
			CoroutineManager.StartCoroutine(UpdateFreeze);
		}
		
		public FreezingComponent(Entity Parent, float TotalTime, float TotalDamage) : base(Parent){
			this.TotalTime = TotalTime;
			this.TotalDamage = TotalDamage;
			CoroutineManager.StartCoroutine(UpdateFreeze);
		}
		
		public override void Update(){}
		
		public IEnumerator UpdateFreeze(){
			Parent.Model.BaseTint = Bar.Blue * new Vector4(1,1,2,1) * .7f;
			OldSpeed = Parent.Speed;
			Parent.Speed = 0;
			Parent.Model.Pause = true;
			while(TotalTime > PTime && !Parent.IsDead && !Disposed){
				
				Time += Engine.Time.ScaledFrameTimeSeconds;
				if(Time >= 1){
					PTime++;
					Time = 0;
					float Exp;
					Parent.Damage( (float) (TotalDamage / TotalTime), Damager, out Exp, true);
					if(Damager != null && Damager is Humanoid)
						(Damager as Humanoid).XP += Exp;
				}
				
				yield return null;
			}
			Parent.Model.BaseTint = Vector4.Zero;
			Parent.Speed = OldSpeed;
			Parent.Model.Pause = false;
			this.Dispose();
			Parent.RemoveComponent(this);
		}
	}
}
