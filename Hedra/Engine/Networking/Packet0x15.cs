/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 12/02/2017
 * Time: 08:26 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Player;

namespace Hedra.Engine.Networking
{
	/// <summary>
	/// Description of Packet0x15.
	/// </summary>
	[Serializable]
	public class Packet0x15
	{
		public float Damage;
		
		public static Packet0x15 From(float Damage){
			Packet0x15 Packet = new Packet0x15();
			Packet.Damage = Damage;
			
			return Packet;
		}
		
		public static void SetValues(Humanoid Human, Packet0x15 Packet){
			float Exp;
			Human.Damage(Packet.Damage, null, out Exp, true);
		}
	}
}
