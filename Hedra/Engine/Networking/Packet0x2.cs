/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/07/2016
 * Time: 08:49 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Runtime.Serialization;
using Hedra.Engine.Player;
using OpenTK;
using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.Networking
{
	/// <summary>
	/// Description of PlayerPacket.
	/// </summary>
	[Serializable]
	internal class Packet0x2
	{
		public string[] MeshFiles;
		public int HairColor, EyeColor;
		public string Name;
		
		public static Packet0x2 FromHuman(Humanoid P){
			Packet0x2 Packet = new Packet0x2();
			Packet.Name = P.Name;
			
			return Packet;
		}
		
		public static void SetValues(Humanoid Human, Packet0x2 Packet){
			Human.RemoveComponent( Human.SearchComponent<HealthBarComponent>() );
			Human.AddComponent( new HealthBarComponent(Human, Packet.Name) );
			Human.Model.UpdateModel();
		}
	}
}
