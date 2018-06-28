/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 01/02/2017
 * Time: 04:30 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Player;

namespace Hedra.Engine.Networking
{
	[Serializable]
	internal class Packet0x8
	{
		public AnimationType Type;
		public bool LightOn = false;
		
		public static Packet0x8 FromHuman(Humanoid Human){
			Packet0x8 Packet = new Packet0x8();
			Packet.LightOn = Human.HandLamp.Enabled;
			if(Human.IsRolling){
				Packet.Type = AnimationType.ROLL;
			}else{
				if(Human.IsAttacking){
					//if(Human.Model.LeftWeapon.a)
					Packet.Type = AnimationType.ATTACK1;
				}else{
					if(Human.IsMoving)
						Packet.Type = AnimationType.RUN;
					else
						Packet.Type = AnimationType.IDLE;
				}
			}
			return Packet;
		}
		
		public static void SetValues(Humanoid Human, Packet0x8 Packet){
			if(Packet.Type == AnimationType.IDLE)
				Human.Model.Idle();
			else if(Packet.Type == AnimationType.RUN)
				Human.Model.Run();
			else if(Packet.Type == AnimationType.ROLL)
				Human.Roll();
			else if(Packet.Type == AnimationType.ATTACK1)
				Human.Model.LeftWeapon.Attack1(Human);
			Human.HandLamp.Enabled = Packet.LightOn;
		}
	}
	
	public enum AnimationType{
		IDLE,
		RUN,
		ATTACK1,
		ATTACK2,
		ROLL
	}
}
