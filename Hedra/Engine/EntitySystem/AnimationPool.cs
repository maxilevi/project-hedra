/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 18/06/2016
 * Time: 08:07 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.EntitySystem
{
	/// <summary>
	/// Description of AnimationPool.
	/// </summary>
	public static class AnimationPool
	{
		public static void HorseAttack(AttackAnimation Data, ref float FTime, ref bool IsAttacking){
			Data.CoolDown -= Time.ScaledFrameTimeSeconds;
			
			if(Data.Attack1 && Data.CoolDown < 0){
				float Range = 1.0f;
				float Cos = ((float) Math.Cos(FTime * 2.25f * Mathf.Radian + Math.PI) + 1f ) * 0.5f;
				FTime+=1.25f * (float) Time.deltaTime * 60;
				
				Data.RightHand.TargetRotation = new Vector3(1-Cos*95,0,0);
				Data.LeftHand.TargetRotation = new Vector3(1-Cos*95,0,0);
				Data.Body.TargetRotation = new Vector3(1-Cos * 55,0,0);
				Data.Body.TargetPosition = new Vector3(0,Cos*0.9f,Cos* 1.8f);
				Data.Head.TargetRotation = new Vector3(1-Cos * 55,0,0);
				Data.Head.TargetPosition = new Vector3(0,Cos*0.9f,Cos* 1.8f);
				Data.RightHand.TargetPosition = new Vector3(0,Cos * 2.2f,Cos*0.6f);
				Data.LeftHand.TargetPosition = new Vector3(0,Cos * 2.2f,Cos*0.6f);
				
				Data.RightFoot.TargetPosition = new Vector3(0,0,Cos * 0.4f);
				Data.LeftFoot.TargetPosition = new Vector3(0,0,Cos * 0.4f);
				Data.RightFoot.TargetRotation = new Vector3(-Cos * 15,0,0);
				Data.LeftFoot.TargetRotation = new Vector3(-Cos * 15,0,0);
				
				if(Cos > 0.5f&& !Data.Damaged){
					float Exp;
			        Data.Target.Damage(Data.Damage, Data.Parent, out Exp);
			        Data.Damaged = true;
				}
				if(Cos < 0.001f && FTime > 120){
			        Data.CoolDown = .5f;
			        Data.Attack1 = false;
					IsAttacking = false;
					Data.Damaged = false;
					FTime = 100;
				}
			}		
		}
		
		public static void BiteAttack(AttackAnimation Data, ref float FTime, ref bool IsAttacking){
			Data.CoolDown -= Time.ScaledFrameTimeSeconds;
			
			if(Data.CoolDown < 0){
				float Range = 1.2f;
				float Cos = 1 * Range - ((float) Math.Cos(FTime * 2.75f * Mathf.Radian + Math.PI) + 1f) * 0.5f * Range;
				FTime+=1.75f * (float) Time.deltaTime * 60;
				
				Data.Body.AnimationPosition = new Vector3(0,0,0.5f*Cos);
				Data.Head.AnimationPosition = new Vector3(0,0,0.5f*Cos);
				Data.RightFoot.AnimationRotation = new Vector3(Cos*15,0,0);
				Data.LeftFoot.AnimationRotation = new Vector3(Cos*15,0,0);
				Data.LeftHand.AnimationRotation = new Vector3(Cos*15,0,0);
				Data.RightHand.AnimationRotation = new Vector3(Cos*15,0,0);
				//Data.Body.AnimationRotation = new Vector3(-7.5f*Cos,0,0);
				
				if(Cos > 0.5f * Range && !Data.Damaged){
					float Exp;
			        Data.Target.Damage(Data.Damage, Data.Parent, out Exp);
			        Data.Damaged = true;
				}
				if(Cos < 0.001f * Range && FTime > 130){
					
			        Data.CoolDown = .75f;
			        Data.Attack1 = false;
					IsAttacking = false;
					Data.Damaged = false;
					FTime = 100;
				}
			}
		}
		
		
		public static void RatAttack(AttackAnimation Data, ref float FTime, ref bool IsAttacking){
			Data.CoolDown -= Time.ScaledFrameTimeSeconds;
			
			if(Data.CoolDown < 0){
				float Range = 1.2f;
				float Cos = 1 * Range - ((float) Math.Cos(FTime * 2.25f * Mathf.Radian + Math.PI) + 1f) * 0.5f * Range;
				FTime+=3.00f * (float) Time.deltaTime * 60;
				
				Data.Body.AnimationPosition = new Vector3(0,0,2 *Cos);
				Data.Body.AnimationRotation = new Vector3(-7.5f*Cos,0,0);
				
				if(Cos > 0.5f * Range && !Data.Damaged){
					float Exp;
			        Data.Target.Damage(Data.Damage, Data.Parent, out Exp);
			        Data.Damaged = true;
				}
				if(Cos < 0.001f * Range && FTime > 130){
					
			        Data.CoolDown = .75f;
			        Data.Attack1 = false;
					IsAttacking = false;
					Data.Damaged = false;
					FTime = 100;
				}
			}
		}
		
		public static void FlyingMobAttack(AttackAnimation Data, ref float FTime, ref bool IsAttacking){
			Data.CoolDown -= Time.ScaledFrameTimeSeconds;
			
			if(Data.CoolDown < 0){
				float Range = 1.2f;
				float Cos = 1 * Range - ((float) Math.Cos(FTime * 2.25f * Mathf.Radian + Math.PI) + 1f) * 0.5f * Range;
				FTime+=3f * (float) Time.deltaTime * 60;
				
				Data.Body.AnimationPosition = new Vector3(0,0,2*Cos);
				Data.Body.AnimationRotation = new Vector3(35*Cos,0,0);
				
				if(Cos > 0.5f * Range && !Data.Damaged){
					float Exp;
			        Data.Target.Damage(Data.Damage, Data.Parent, out Exp);
			        Data.Damaged = true;
				}
				if(Cos < 0.001f * Range && FTime > 130){
					
			        Data.CoolDown = .75f;
			        Data.Attack1 = false;
					IsAttacking = false;
					Data.Damaged = false;
					FTime = 100;
				}
			}
		}
		public static void SnowmanAttack(AttackAnimation Data, ref float FTime, ref bool IsAttacking){
			Data.CoolDown -= Time.ScaledFrameTimeSeconds;
			
			if(Data.Attack1 && Data.CoolDown < 0){
				float Range = 1.2f;
				float Cos = 1 * Range - ((float) Math.Cos(FTime * 2.25f * Mathf.Radian + Math.PI) + 1f) * 0.5f * Range;
				FTime+=3f * (float) Time.deltaTime * 60;
				
				Data.Body.AnimationRotation = new Vector3(15*Cos,0,0);
				Data.Head.AnimationRotation = new Vector3(15*Cos,-15*Cos,0);
				
				Data.RightHand.AnimationRotation = new Vector3(15*Cos,0,0);
				Data.LeftHand.AnimationRotation = new Vector3(15*Cos,0,0);
				
				if(Cos > 0.5f * Range && !Data.Damaged){
					float Exp;
			        Data.Target.Damage(Data.Damage, Data.Parent, out Exp);
			        Data.Damaged = true;
				}
				if(Cos < 0.001f * Range && FTime > 130){
					
					Data.RightHand.AnimationRotation = Vector3.Zero;
					Data.Head.AnimationRotation = Vector3.Zero;
					Data.LeftHand.AnimationRotation = Vector3.Zero;
					Data.Body.AnimationRotation = Vector3.Zero;
					
					
			        Data.CoolDown = .75f;
			        Data.Attack1 = false;
					IsAttacking = false;
					Data.Damaged = false;
					FTime = 100;
				}
			}
		}
		
		public static void SpiderAttack(SpiderAttackAnimation Data, ref float FTime, ref bool IsAttacking){
			Data.CoolDown -= Time.ScaledFrameTimeSeconds;
			
			if(Data.Attack1 && Data.CoolDown < 0){
				float Range = 1.2f;
				float Cos = 1 * Range - ((float) Math.Cos(FTime * 2.25f * Mathf.Radian + Math.PI) + 1f) * 0.5f * Range;
				FTime+=3f * (float) Time.deltaTime * 60;
				
				Data.Body.AnimationPosition = new Vector3(0, 0, 1.4f*Cos );
				Data.Head.AnimationPosition = new Vector3(0, 0, 1.4f*Cos );
				
				Data.RightHand.AnimationRotation = new Vector3(-90*Cos, 0, 0);
				Data.RightHand.AnimationPosition = new Vector3(1.8f*Cos, 0, 1.4f*Cos );
				
				Data.LeftHand.AnimationRotation = new Vector3(-90*Cos, 0, 0);
				Data.LeftHand.AnimationPosition = new Vector3(-1.8f*Cos, 0, 1.4f*Cos );

				Data.LeftFoot.AnimationRotation = new Vector3(15*Cos,0,0);
				Data.RightFoot.AnimationRotation = new Vector3(15*Cos,0,0);
				Data.LeftFoot.AnimationPosition = new Vector3(0,-.5f * Cos, 1.4f*Cos);
				Data.RightFoot.AnimationPosition = new Vector3(0,-.5f * Cos, 1.4f*Cos);
				
				Data.MiddleRight.AnimationPosition = new Vector3(0, 0, 1.4f*Cos);
				Data.MiddleRight.AnimationRotation = new Vector3(7.5f*Cos,0,0);
				
				Data.MiddleLeft.AnimationPosition = new Vector3(0, 0, 1.4f*Cos);
				Data.MiddleLeft.AnimationRotation = new Vector3(7.5f*Cos,0,0);
				
				if(Cos > 0.5f * Range && !Data.Damaged){
					float Exp;
			        Data.Target.Damage(Data.Damage, Data.Parent, out Exp);
			        Data.Damaged = true;
				}
				if(Cos < 0.001f * Range && FTime > 130){
					
					Data.RightHand.AnimationRotation = Vector3.Zero;
					Data.RightHand.AnimationPosition = Vector3.Zero;
					Data.LeftHand.AnimationRotation = Vector3.Zero;
					Data.LeftHand.AnimationPosition = Vector3.Zero;	
					
			        Data.CoolDown = .75f;
			        Data.Attack1 = false;
					IsAttacking = false;
					Data.Damaged = false;
					FTime = 100;
				}
			}
		}
		
		public static void FrontFourLeggedAttack(AttackAnimation Data, ref float FTime, ref bool IsAttacking){
			Data.CoolDown -= Time.ScaledFrameTimeSeconds;
			
			if(Data.Attack1 && Data.CoolDown < 0){
				float Range = 1.2f;
				float Cos = ((float) Math.Cos(FTime * 2.25f * Mathf.Radian + Math.PI) * Range + 1f * Range) * 0.5f;
				FTime+=1.25f * (float) Time.deltaTime * 60;

				Data.LeftHand.AnimationRotationPoint = Vector3.Zero;
				Data.RightFoot.AnimationRotationPoint = Vector3.Zero;
				Data.RightHand.AnimationRotationPoint = Vector3.Zero;
				Data.LeftHand.AnimationRotationPoint = Vector3.Zero;
				
				Data.RightHand.TargetRotation = new Vector3(-Cos*75,0,0);
				Data.LeftHand.TargetRotation = new Vector3(-Cos*75,0,0);
				Data.Body.TargetRotation = new Vector3(-Cos * 55,0,0);
				Data.Body.TargetPosition = new Vector3(0,Cos*0.7f,0);
				Data.Head.TargetPosition = new Vector3(0,Cos * Data.HeadUp,-Cos*0.5f);
				Data.RightHand.TargetPosition = new Vector3(0,Cos * 0.4f,Cos*0.6f);
				Data.LeftHand.TargetPosition = new Vector3(0,Cos * 0.4f,Cos*0.6f);
				
				Data.RightFoot.TargetPosition = new Vector3(0,0,Cos * 0.4f);
				Data.LeftFoot.TargetPosition = new Vector3(0,0,Cos * 0.4f);
				Data.RightFoot.TargetRotation = new Vector3(Cos * 15,0,0);
				Data.LeftFoot.TargetRotation = new Vector3(Cos * 15,0,0);
				
				if(Cos > 0.5f * Range && !Data.Damaged){
					float Exp;
			       	Data.Target.Damage(Data.Damage, Data.Parent, out Exp);
			        Data.Damaged = true;
				}
				if(Cos < 0.001f * Range && FTime > 120){
					Data.LeftFoot.AnimationRotationPoint = Data.PrevLegPoint;
					Data.RightFoot.AnimationRotationPoint = Data.PrevLegPoint;
					Data.LeftHand.AnimationRotationPoint = Data.PrevLegPoint;
					Data.RightHand.AnimationRotationPoint = Data.PrevLegPoint;
					
					Data.Head.TargetPosition = Vector3.Zero;
					Data.Body.TargetPosition = Vector3.Zero;
					Data.Body.TargetRotation = Vector3.Zero;
					Data.RightFoot.TargetRotation = Vector3.Zero;
					Data.LeftFoot.TargetRotation = Vector3.Zero;
					Data.RightHand.TargetRotation = Vector3.Zero;
					Data.LeftHand.TargetRotation = Vector3.Zero;
					
					Data.RightFoot.TargetPosition = Vector3.Zero;
					Data.LeftFoot.TargetPosition = Vector3.Zero;
					Data.RightHand.TargetPosition = Vector3.Zero;
					Data.LeftHand.TargetPosition = Vector3.Zero;
					
			        Data.CoolDown = .5f;
			        Data.Attack1 = false;
					IsAttacking = false;
					Data.Damaged = false;
					FTime = 100;
				}
			}		
		}
	
		public class AttackAnimation{
			public Entity Parent, Target;
			public bool Attack1, Attack2, Damaged;
			public ObjectMesh LeftHand, RightHand, LeftFoot, RightFoot, Head, Body;
			public Vector3 PrevLegPoint;
			public float Damage, CoolDown, HeadUp = 1.1f;
		}
		
		public class SpiderAttackAnimation{
			public Entity Parent, Target;
			public bool Attack1, Attack2, Damaged;
			public ObjectMesh LeftHand, RightHand, LeftFoot, RightFoot, Head, Body, MiddleRight, MiddleLeft;
			public float Damage, CoolDown;
		}
		
	}
}
