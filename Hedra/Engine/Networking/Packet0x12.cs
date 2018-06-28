/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 11/02/2017
 * Time: 05:49 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.Player;
using System.Collections.Generic;

namespace Hedra.Engine.Networking
{
	[Serializable]
	internal class Packet0x12
	{
		public ushort[] Seeds;
		public byte[] MobTypes;
		public Vector3[] Position;
		
		public static Packet0x12 FromHuman(Humanoid Human){
			Packet0x12 Packet = new Packet0x12();
			/*
			List<byte> MobTypes = new List<byte>();
			List<Vector3> Position = new List<Vector3>();
			
			for(int i = Human.World.Entities.Count-1; i > -1; i--){
				//Check if it was spawned through World.SpawnMob
				if(Human.World.Entities[i].MobId != 0){
					Ids.Add( (ushort) Human.World.Entities[i].MobId);
					MobTypes.Add( (byte) Human.World.Entities[i].MobType);
					Health.Add( Human.World.Entities[i].MaxHealth);
					Position.Add( Human.World.Entities[i].BlockPosition);
					
				}
			}
			Packet.Ids = Ids.ToArray();
			Packet.MobTypes = MobTypes.ToArray();
			Packet.Health = Health.ToArray();
			Packet.Position = Position.ToArray();*/
			return Packet;
		}
		
		public static void SetValues(Humanoid Human, Packet0x12 Packet){

		}
	}
}
