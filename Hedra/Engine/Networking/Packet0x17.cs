/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 13/02/2017
 * Time: 01:43 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Management;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Item;
using Hedra.Engine.Player;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Hedra.Engine.Generation;

namespace Hedra.Engine.Networking
{
	/// <summary>
	/// Description of Packet0x17.
	/// </summary>
	[Serializable]
	public class Packet0x17
	{
		public ushort ItemId;
		
		public static Packet0x17 From(ushort ItemId){
			Packet0x17 Packet = new Packet0x17();
			Packet.ItemId = ItemId;
			
			return Packet;
		}
		
		public static PacketBuilder Authorize(Humanoid Human, Packet0x17 Packet){
			PacketBuilder PBuilder = new PacketBuilder();
			
			bool Authorized = false;
			for(int i = World.Items.Count-1; i > -1; i--){
				if(World.Items[i].ItemId == Packet.ItemId){
					World.Items[i].Dispose();
					Authorized = true;
					break;
				}
			}
			if(!Authorized) return null;
			
			using(MemoryStream Ms = new MemoryStream()){
				BinaryFormatter Formatter = new BinaryFormatter();
				Formatter.Serialize(Ms, Packet);
				PBuilder.ID = 0x17;
				PBuilder.Data = ZipManager.ZipBytes(Ms.ToArray());
			}
			return PBuilder;
		}
		
		public static void SetValues(LocalPlayer Human, Packet0x17 Packet){
			for(int i = World.Items.Count-1; i > -1; i--){
				if(World.Items[i].ItemId == Packet.ItemId){
					Human.Inventory.AddItem(World.Items[i].Item);
					Sound.SoundManager.PlaySound(Sound.SoundType.NotificationSound, World.Items[i].Position, false, 1f, 1f);
					UpdateManager.Remove(World.Items[i]);
					World.Items[i].Dispose();
					break;
				}
			}
		}
	}
}
