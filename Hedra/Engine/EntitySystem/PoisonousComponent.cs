/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 05/09/2016
 * Time: 10:13 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using Hedra.Engine.Player;
using Hedra.Engine.Management;
using System.Collections;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.EntitySystem
{
    /// <inheritdoc cref="IEffectComponent" />
    /// <summary>
    /// Description of PoisonousComponent.
    /// </summary>
    public class PoisonousComponent : EntityComponent, IEffectComponent
    {
		public int Chance { get; set; } = 20;
		public float TotalStrength { get; set; } = 60;
		public float BaseTime { get; set; } = 5;
		
		private float _cooldown;
		private bool _canPoison = true;
		
		public PoisonousComponent(Entity Parent) : base(Parent){
			Parent.OnAttacking += this.Apply;
		}
		
		public override void Update(){
			_cooldown -= Time.FrameTimeSeconds;
		}
		
		public void Apply(Entity Victim, float Amount){
			if(_cooldown > 0 || !_canPoison) return;
			
			bool shouldPoison = Utils.Rng.NextFloat() <= Chance * 0.01 ? true : false;
			if(shouldPoison){
				_poisonTime =  5 + Utils.Rng.NextFloat() * 4 -2f;
				_time = 0;
				_pTime = 0;
				this._victim = Victim;
				
				CoroutineManager.StartCoroutine(PoisonCoroutine);
			}
		}
		
		private float _poisonTime = 0;
		private float _time = 0;
		private float _pTime = 0;
		private Entity _victim;
		public IEnumerator PoisonCoroutine(){
			_victim.Model.BaseTint = Bar.Poison *new Vector4(1,3,1,1);
			this._canPoison = false;
			while(_poisonTime > _pTime && !_victim.IsDead && !Disposed){
				
				_time += Engine.Time.FrameTimeSeconds;
				if(_time >= 1){
					_pTime++;
					_time = 0;
					float exp;
					_victim.Damage(TotalStrength / _poisonTime, this.Parent, out exp, true);
					if(Parent is LocalPlayer)
						(Parent as LocalPlayer).XP += exp;
				}
				
				yield return null;
			}
			_victim.Model.BaseTint = Vector4.Zero;
			this._canPoison = true;
			this._cooldown = 4;
		}
	}
}
