/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 13/02/2017
 * Time: 12:47 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using OpenTK;
using Hedra.Engine.Player;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Item;

namespace Hedra.Engine.Networking
{
	[Serializable]
	public class Packet0x16
	{
		public ushort[] Ids;
		public Vector3[] Positions;
		public InventoryItem[] Datas;
		
		public static void Accept(Humanoid Human, Packet0x16 Packet){

			bool Accepted = true;
			for(int i = World.Items.Count-1; i > -1; i--){
				if(World.Items[i].ItemId == Packet.Ids[0]){
					Accepted = false;
					break;
				}
			}
			if(Accepted){
				ItemModel Model = World.DropItem(Packet.Datas[0], Packet.Positions[0]);
				Model.ItemId = Packet.Ids[0];
			}
		}
		
		
		public static Packet0x16 FromHuman(Humanoid Human){
			Packet0x16 Packet = new Packet0x16();
			
			List<Vector3> Positions = new List<Vector3>();
			List<ushort> Ids = new List<ushort>();
			List<InventoryItem> Datas = new List<InventoryItem>();
			
			for(int i = World.Items.Count-1; i > -1; i--){
				Positions.Add(World.Items[i].Position);
				Ids.Add(World.Items[i].ItemId);
				Datas.Add(World.Items[i].Item);
			}
			
			Packet.Positions = Positions.ToArray();
			Packet.Ids = Ids.ToArray();
			Packet.Datas = Datas.ToArray();
			return Packet;
		}
		
		public static void SetValues(Humanoid Human, Packet0x16 Packet){
			lock(World.Items){
				for(int i = 0; i < Packet.Ids.Length; i++){
					bool ItemFound = false;
					ItemModel Item = null;
					for(int j = World.Items.Count-1; j > -1; j--){
						if(World.Items[j].ItemId == Packet.Ids[i]){
							ItemFound = true;
							Item = World.Items[j];
							break;
						}
					}
					if(ItemFound){
						//Update
						Item.Position = Packet.Positions[i];
					}else{
						//Create & Update
						ItemModel NewItem = new ItemModel(Packet.Datas[i], Packet.Positions[i]);
						NewItem.ItemId = Packet.Ids[i];
						
						int k = i;
						NewItem.OnPickup += delegate(LocalPlayer Player) {
							NetworkManager.SendPacket0x17(Packet.Ids[k]);
						};
						World.Items.Add(NewItem);
					}
				}
				//Remove old entities
				for(int j = World.Items.Count-1; j > -1; j--){
					if(World.Items[j].ItemId != 0){
						bool Found = false;
						for(int i = 0; i < Packet.Ids.Length; i++){
							if(Packet.Ids[i] == World.Items[j].ItemId){
								Found = true;
								break;
							}
						}
						if(!Found){
							World.Items[j].Dispose();
							World.Items.RemoveAt(j);
						}
					}
				}
			}
		}
		
		
	}
}
