/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 15/08/2016
 * Time: 12:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Player;
using Hedra.Engine.EntitySystem;
using OpenTK;

namespace Hedra.Engine.QuestSystem
{
	/// <summary>
	/// Description of BoarBossAIComponent.
	/// </summary>
	public class BoarBossAIComponent : BossAIComponent
	{
		public Action AttackMode;
		public BoarBossAIComponent(Entity Parent) : base (Parent){
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
			SpinTarget = (Player.Position - Parent.Position) + SpinDirection * 16f;
			SpinDelta = 0f;
			AttackMode = () => Spin();
		}
		
		private float SpinDelta = 0f;
		public void Spin(){
			Parent.Physics.Move(SpinDirection.Xz.ToVector3() * Parent.Speed * 4f * 8);
			SpinDelta += (SpinDirection.Xz.ToVector3() * 2f * 4f * 8 * (float) Time.deltaTime).LengthFast;
			if(SpinDelta >= SpinTarget.Xz.LengthFast){
				this.IsSpinning = false;
				this.SpinCooldown = 16f;
				this.AttackMode = () => NoAbility();
			}
			Vector3 Dir = (Player.BlockPosition-Parent.BlockPosition).Normalized();
			Parent.Orientation = Dir.Xz.Normalized().ToVector3();
			 
			Matrix4 MV = Mathf.RotationAlign(Vector3.UnitZ, Parent.Orientation);
			Vector3 Axis;
			float Angle;
			MV.ExtractRotation().ToAxisAngle(out Axis, out Angle);	
			Parent.Model.TargetRotation = new Vector3(Parent.Model.TargetRotation.X, Axis.Y * Angle * Mathf.Degree, Parent.Model.TargetRotation.Z);
			Parent.Model.Run();
			
			if((Player.Position-Parent.Position).LengthSquared < Radius && !Player.IsDead){
				float Exp;
				Player.Damage( this.AttackDamage * 2f * (float) Time.deltaTime, this.Parent, out Exp, false);
				//Parent.Model.Idle();
			    return;     
			}
		}
	}
}
