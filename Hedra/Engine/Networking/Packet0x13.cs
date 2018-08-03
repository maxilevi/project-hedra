/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 12/02/2017
 * Time: 07:35 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Player;
using System.Net;
using Hedra.Engine.Generation;

namespace Hedra.Engine.Networking
{
	[Serializable]
	public class Packet0x13
	{
		public ushort MobId;
		public float Damage;
		public byte EntityType;
		
		public static Packet0x13 FromEntity(IEntity Mob, float Damage){
			Packet0x13 Packet = new Packet0x13();
			Packet.MobId = (ushort) Mob.MobId;
			Packet.Damage = Damage;
			
			return Packet;
		}
		
		public static void SetValues(Humanoid Human, Packet0x13 Packet, IPEndPoint IP){
			lock(World.Entities){
				for(int i = World.Entities.Count-1; i > -1; i--){
					if(World.Entities[i].MobId == Packet.MobId){
						if(!World.Entities[i].IsDead){
							float Exp;
							PeerData AttackerData = NetworkManager.PeerDataFromIP(IP);
							if(AttackerData != null){
								World.Entities[i].Damage(Packet.Damage, AttackerData.Human, out Exp, true);
								//Send the XP
								if(Exp != 0)
									NetworkManager.SendPacket0x14(Exp, IP);
								
							}
							break;
						}
					}
				}
			}
		}
		
		
	}
}
