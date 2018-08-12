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
	public class Packet0x8
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
					//if(Human.LeftWeapon.a)
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
	
	}
	
	public enum AnimationType{
		IDLE,
		RUN,
		ATTACK1,
		ATTACK2,
		ROLL
	}
}
