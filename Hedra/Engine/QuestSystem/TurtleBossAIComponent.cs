/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 30/07/2016
 * Time: 01:14 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.Player;
using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.QuestSystem
{
	/// <summary>
	/// Description of TurtleBossAIComponent.
	/// </summary>
	public class TurtleBossAIComponent : BossAIComponent
	{
		public Action AttackMode;
		public TurtleBossAIComponent(Entity Parent) : base (Parent){
			AttackMode = () => NoAbility();
			base.AILogic = delegate{};
		}
		
		public override void LateUpdate(){
			Player = GameManager.Player;
			if(Player == null) return;
			

			if(!IsSpinning && SpinCooldown < 0 && (Player.Position - Parent.Position).LengthSquared < VisiblityRange){
				StartSpin();
			}

			
			AttackMode();
			SpinCooldown -= Time.FrameTimeSeconds;
			
		}
		
		private float SpinCooldown = 8f;
		private Vector3 SpinTarget, SpinDirection;
		private bool IsSpinning;
		public void StartSpin(){
			this.IsSpinning = true;
			SpinDirection = (Player.Position - Parent.Position).NormalizedFast();
			SpinTarget = (Player.Position - Parent.Position) + SpinDirection * 36f;
			SpinDelta = 0f;
			AttackMode = () => Spin();
		}
		
		private float SpinDelta = 0f;
		public void Spin(){
			IsSpinning = true;
			
			Parent.Physics.Move(SpinDirection.Xz.ToVector3() * 2f * 4f * 8 * (float)Time.deltaTime);
			SpinDelta += (SpinDirection.Xz.ToVector3() * 2f * 4f * 8 * (float) Time.deltaTime).LengthFast;
			if(SpinDelta >= SpinTarget.Xz.LengthFast ){
				this.IsSpinning = false;
				this.SpinCooldown = 16f;
				this.AttackMode = () => NoAbility();
			}
			Parent.Model.TargetRotation += Vector3.UnitY * (float) Time.deltaTime * 1000f;
			
			if((Player.Position-Parent.Position).LengthSquared < Radius && !Player.IsDead){
				float Exp;
				Player.Damage( this.AttackDamage * 2f * (float) Time.deltaTime, this.Parent, out Exp, false);
			    return;     
			}
		}	
	}
}
