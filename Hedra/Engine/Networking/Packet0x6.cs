/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 01/02/2017
 * Time: 01:30 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;

namespace Hedra.Engine.Networking
{
	/// <summary>
	/// Description of Packet0x6.
	/// </summary>
	[Serializable]
	public class Packet0x6
	{
		public int Seed;
		public float DayTime = 12000;
		
		public static Packet0x6 FromHuman(Humanoid Human){
			Packet0x6 Packet = new Packet0x6();
			Packet.Seed = World.Seed;
			Packet.DayTime = EnvironmentSystem.SkyManager.DayTime;
			
			return Packet;
		}
		
		public static void SetValues(Humanoid Human, Packet0x6 Packet){
			NetworkManager.WorldSeed = Packet.Seed;
			NetworkManager.WorldTime = Packet.DayTime;
		    World.Recreate(Packet.Seed);
			EnvironmentSystem.SkyManager.SetTime(Packet.DayTime);
		}
	}
}
