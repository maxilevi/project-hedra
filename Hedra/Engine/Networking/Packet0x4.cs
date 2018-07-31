/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 25/07/2016
 * Time: 01:02 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Player;
using OpenTK;

namespace Hedra.Engine.Networking
{
	/// <summary>
	/// Description of Packet0x4.
	/// </summary>
	[Serializable]
	public class Packet0x4
	{
		public static Vector3 PrevPosition, PrevRotation;
		public Vector3 Position, Rotation;
		
		public static Packet0x4 FromHuman(Humanoid P){
			Packet0x4 Packet = new Packet0x4();
			Packet.Position = P.BlockPosition;
			Packet.Rotation = P.Rotation;
			return Packet;
		}
		
		public static void SetValues(Humanoid Human, Packet0x4 Packet){
			Human.Position = Packet.Position;
			Human.Rotation = Packet.Rotation;
		}
	}
}
