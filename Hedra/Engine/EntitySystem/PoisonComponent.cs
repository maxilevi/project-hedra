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
using Hedra.Engine.Rendering.Particles;

namespace Hedra.Engine.EntitySystem
{
	/// <summary>
	/// Description of BurningComponent.
	/// </summary>
	public class PoisonComponent : EntityComponent
	{
		private float TotalTime, Time, TotalDamage;
		private int PTime;
		private Entity Damager = null;
		
		public PoisonComponent(Entity Parent, Entity Damager, float TotalTime, float TotalDamage) : base(Parent)
        {
			this.TotalTime = TotalTime;
			this.TotalDamage = TotalDamage;
			this.Damager = Damager;
			CoroutineManager.StartCoroutine(UpdatePoison);
		}
		
		public PoisonComponent(Entity Parent, float TotalTime, float TotalDamage) : base(Parent)
        {
			this.TotalTime = TotalTime;
			this.TotalDamage = TotalDamage;
			CoroutineManager.StartCoroutine(UpdatePoison);
		}
		
		public override void Update(){}
		
		public IEnumerator UpdatePoison(){
			Parent.Model.BaseTint = Bar.Poison *new Vector4(1,3,1,1);
			while(TotalTime > PTime && !Parent.IsDead && !Disposed){
				
				Time += Engine.Time.DeltaTime;
				if(Time >= 1){
					PTime++;
					Time = 0;
				    Parent.Damage(TotalDamage / TotalTime, Damager, out float Exp, true);
					if(Damager is Humanoid humanoid)
						humanoid.XP += Exp;
				}
				
				yield return null;
			}
			Parent.Model.BaseTint = Vector4.Zero;
			this.Dispose();
			Parent.RemoveComponent(this);
		}
	}
}
