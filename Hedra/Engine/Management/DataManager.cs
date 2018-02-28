/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 15/06/2016
 * Time: 05:36 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using System.IO;
using System.Runtime.InteropServices;
using Hedra.Engine.Player;
using Hedra.Engine.Generation;
using Hedra.Engine.Item;
using System.Collections.Generic;

namespace Hedra.Engine.Management
{
	/// <summary>
	/// Description of SaveManager.
	/// </summary>
	public static class DataManager
	{
		public const float SaveVersion = 0.9f;
		
		public static void SavePlayer(PlayerData Player){
			string ChrFile = AssetManager.AppData+"/Characters/"+Player.Name;
			
			if(File.Exists(ChrFile+".db")){
				if(File.Exists(ChrFile+".db.bak"))
					File.Delete(ChrFile+".db.bak");
				File.Copy(ChrFile+".db", ChrFile+".db.bak");
				
				FileInfo FInfo = new FileInfo(ChrFile+".db.bak");
				FInfo.Attributes |= FileAttributes.Hidden;
			}
			
			using (FileStream Fs = File.Create(AssetManager.AppData+"/Characters/"+Player.Name+".db")){
				using(BinaryWriter Bw = new BinaryWriter(Fs)){
					Bw.Write(SaveVersion);
					Bw.Write(Player.Name);
					Bw.Write(Player.BlockPosition.X);
					Bw.Write(Player.BlockPosition.Y+4);
					Bw.Write(Player.BlockPosition.Z);
					
					Bw.Write(Player.Rotation.X);
					Bw.Write(Player.Rotation.Y);
					Bw.Write(Player.Rotation.Z);
					
					Bw.Write(Player.Speed);
					
					Bw.Write(Player.RandomFactor);
					Bw.Write(Player.AddonHealth);
					Bw.Write(Player.Health);
					
					Bw.Write(Player.XP);			
					Bw.Write(Player.Level);
					
					Bw.Write(Player.Mana);
					
					Bw.Write(Player.Color0);
					Bw.Write(Player.Color1);
					
					Bw.Write(Player.WorldSeed);			
					
					Bw.Write(Player.SkillsData.Length);
					Bw.Write(Player.SkillsData);
					
					Bw.Write(Player.SkillIDs.Length);
					Bw.Write(Player.SkillIDs);
					
					Bw.Write(Player.TargetPosition);
					
					Bw.Write(Player.QuestData);
					Bw.Write(Player.Daytime);
					Bw.Write( (int) Player.ClassType);
					
					if(Player.Items != null){
						foreach(KeyValuePair<InventoryItem,int> Pair in Player.Items){
							byte[] ItemData = DataManager.RawSerialize(Pair.Key.Info);
							
							Bw.Write(Pair.Key.Name);
							Bw.Write((int)Pair.Key.Type);
							Bw.Write(Pair.Value);
							Bw.Write(ItemData.Length);
							Bw.Write(ItemData);
						}
					}
				}
			}
		}
	
		public static PlayerData DataFromPlayer(LocalPlayer P){
			bool Connected = Networking.NetworkManager.IsConnected;
		    var Data = new PlayerData
		    {
		        Level = P.Level,
		        Health = P.Health,
		        Mana = P.MaxXP,
		        XP = P.XP,
		        RandomFactor = P.RandomFactor,
		        WorldSeed = (Connected) ? Scenes.SceneManager.Game.CurrentData.WorldSeed : World.Seed,
		        Name = P.Name,
		        Rotation = P.Rotation,
		        BlockPosition = (Connected) ? Scenes.SceneManager.Game.CurrentData.BlockPosition : P.BlockPosition,
		        AddonHealth = P.AddonHealth,
		        Color0 = P.Model.Color1,
		        Color1 = P.Model.Color0,
		        SkillsData = P.SkillSystem.Save(),
		        SkillIDs = P.Skills.Save(),
		        TargetPosition = P.Physics.TargetPosition,
		        Daytime = (Connected) ? Scenes.SceneManager.Game.CurrentData.Daytime : Enviroment.SkyManager.DayTime,
		        QuestData = (Connected) ? "" : World.QuestManager.Quest.Serialize(),
		        ClassType = P.ClassType
		    };
		    var items = new Dictionary<InventoryItem, int>();
			for(int i = 0; i < P.Inventory.Items.Length; i++){
				if(P.Inventory.Items[i] != null && !items.ContainsKey(P.Inventory.Items[i]))
					items.Add(P.Inventory.Items[i], i);
			}
			Data.Items = items;	
			return Data;
		}
		
		public static PlayerData LoadPlayer(string FileName){
			
			FileInfo ChrInfo = new FileInfo(FileName);
			if(ChrInfo.Length == 0){
				if(File.Exists(FileName+".bak")){
					File.Delete(FileName);
					File.Copy(FileName+".bak", FileName);
					FileInfo FInfo = new FileInfo(FileName);
					FInfo.Attributes = FileAttributes.Normal;
				}
			}
			
			return DataManager.LoadPlayer(File.Open(FileName, FileMode.Open));
		}
		
		public static PlayerData LoadPlayer(Stream str){
			PlayerData Data = new PlayerData();
			Dictionary<InventoryItem, int> Items;
			using(BinaryReader Br = new BinaryReader(str)){
				float Version = Br.ReadSingle();
				
				Data.Name = Br.ReadString();
				Data.BlockPosition = new Vector3(Br.ReadSingle(), Br.ReadSingle(), Br.ReadSingle());
				Data.Rotation = new Vector3(Br.ReadSingle(), Br.ReadSingle(), Br.ReadSingle());
				
				Data.Speed = Br.ReadSingle();
				if(Version < 0.82f){
					Data.SpeedMultiplier = Br.ReadSingle();
					Data.Speed /= Data.SpeedMultiplier;
				}
				if(Version >= 0.89f){
					Data.RandomFactor = Br.ReadSingle();
					Data.AddonHealth = Br.ReadSingle();
				}
				Data.Health = Br.ReadSingle();
				
				if(Version < 0.89f)
					Br.ReadSingle();
				
				Data.XP = Br.ReadSingle();
				
				if(Version < 0.89f)
					Br.ReadSingle();
				
				Data.Level = Br.ReadInt32();
				
				Data.Mana = Br.ReadSingle();
				
				if(Version < 0.89f)
					Br.ReadSingle();
				
				if(Version < 0.89f){
					//Read old files
					Br.ReadString();
					Br.ReadString();
					Br.ReadString();
					Br.ReadString();
					Br.ReadString();
					Br.ReadString();
				}
				Data.Color0 = Br.ReadVector4();
				Data.Color1 = Br.ReadVector4();
				
				//Seed cannot be negative anymore
				Data.WorldSeed = Math.Abs(Br.ReadInt32());
				//Skip the heightmap
				if(Version < .81f){
					Br.ReadBytes(Br.ReadInt32());
				}
			    if (Version < .9f)
			    {
                    //old health & mana regen
			        Br.ReadSingle();
			        Br.ReadSingle();
			    }
			    if(Version >= 0.72f){
					Data.SkillsData = Br.ReadBytes(Br.ReadInt32());
				}
				if(Version >= 0.73f){
					if(Version >= .88f){
						Data.SkillIDs = Br.ReadBytes(Br.ReadInt32());
					}else{
						Data.SkillIDs = new byte[4];
						Data.SkillIDs[0] = Br.ReadByte();
						Data.SkillIDs[1] = Br.ReadByte();
						Data.SkillIDs[2] = Br.ReadByte();
						Data.SkillIDs[3] = Br.ReadByte();
					}
				}
				if(Version >= 0.74f && Version < 0.83f){
					Br.ReadSingle();
				}
				if(Version >= 0.79f){
					Data.TargetPosition = Br.ReadVector3();
				}
				if(Version >= 0.75f && Version < 0.84f){
					int Length = Br.ReadInt32();
					Data.Entities = Br.ReadBytes(Length);
				}
				if(Version >= 0.85f)
					Data.QuestData = Br.ReadString();
				
				if(Version >= 0.86f)
					Data.Daytime = Br.ReadSingle();
				
				if(Version >= 0.87f)
					Data.ClassType = (Class) Br.ReadInt32();
				
				Items = new Dictionary<InventoryItem, int>();
				while (Br.BaseStream.Position < Br.BaseStream.Length){
					
					string Name = Br.ReadString();
					ItemType Type = (ItemType) Br.ReadInt32();
					if(Version < 0.8f)
						Br.ReadInt32();
					int Index = Br.ReadInt32();
					byte[] ItemInfoData = Br.ReadBytes(Br.ReadInt32());
					InventoryItem NewItem;
					
					if(Version < 0.78f){
						OldItemInfo Info = RawDeserialize<OldItemInfo>(ItemInfoData,0);
						ItemInfo NewInfo = new ItemInfo(Info.MaterialType, Info.Damage);
						NewItem = new InventoryItem(Type, NewInfo);
					}else{
						ItemInfo Info = RawDeserialize<ItemInfo>(ItemInfoData,0);
						NewItem = new InventoryItem(Type, Info);
					}
					
					if((int)NewItem.Type >= (int) ItemType.MaxItems)
						NewItem.Type = ItemType.Sword;
					Items.Add(NewItem, Index);
				}
				if(Version < 0.8f){ 
					Items.Clear();
					Items.Add(new InventoryItem(ItemType.Sword, ItemInfo.Random(ItemType.Sword) ), Inventory.WeaponHolder);
				}
				//Compatibility
				if(Class.None == Data.ClassType){
					foreach(InventoryItem Item in Items.Keys){
						if(Items[Item] == Inventory.WeaponHolder){
							if(Item.Type == ItemType.ThrowableDagger)
								Data.ClassType = Class.Rogue;
							
							else if(Item.Type == ItemType.Bow)
								Data.ClassType = Class.Archer;
							
							else if(Item.Type == ItemType.Sword)
								Data.ClassType = Class.Warrior;
						}
					}
				}
			}

			Data.Items = Items;
			str.Close();
			str.Dispose();
			return Data;
		}
		
		
		/// <summary>
		/// converts byte[] to struct
		/// </summary>
		public static T RawDeserialize<T>(byte[] rawData, int position)
	    {
	        int rawsize = Marshal.SizeOf(typeof(T));
	        if (rawsize > rawData.Length - position)
	            throw new ArgumentException("Not enough data to fill struct. Array length from position: "+(rawData.Length-position) + ", Struct length: "+rawsize);
	        IntPtr buffer = Marshal.AllocHGlobal(rawsize);
	        Marshal.Copy(rawData, position, buffer, rawsize);
	        T retobj = (T)Marshal.PtrToStructure(buffer, typeof(T));
	        Marshal.FreeHGlobal(buffer);
	        return retobj;
	    }
		
		/// <summary>
		/// converts a struct to byte[]
		/// </summary>
		public static byte[] RawSerialize(object anything)
	    {
	        int rawSize = Marshal.SizeOf(anything);
	        IntPtr buffer = Marshal.AllocHGlobal(rawSize);
	        Marshal.StructureToPtr(anything, buffer, false);
	        byte[] rawDatas = new byte[rawSize];
	        Marshal.Copy(buffer, rawDatas, 0, rawSize);
	        Marshal.FreeHGlobal(buffer);
	        return rawDatas;
	    }
		
		public static PlayerData[] PlayerFiles{
			get{
				List<PlayerData> FilesList = new List<PlayerData>();
				string[] Files = Directory.GetFiles(AssetManager.AppData+"Characters/");

				for(int i = 0; i < Files.Length; i++){
					if(Files[i].EndsWith(".bak")) 
						continue;
					try{
						FilesList.Add( LoadPlayer(Files[i]));
					}catch(Exception e){
						if(e is UnauthorizedAccessException){
							Log.WriteLine("Unathoriazed access in file: "+Path.GetFileName(Files[i]));
							continue;
						}
						Log.WriteLine(e.ToString());
					}
				}
				return FilesList.ToArray();
			}
		}
		
		
		public static int CharacterCount{
			get{ 
				string[] Files = Directory.GetFiles(AssetManager.AppData+"Characters/");
				int Count = 0;
				for(int i = 0; i < Files.Length; i++){
					if(!Files[i].EndsWith(".bak")) Count++;
				}
				return Count;
			}
		}
		
		
		public static string[] GetModelFileNames(string Name){
			List<string> ValidFiles = new List<string>();
			string[] Files = AssetManager.GetFileNames(AssetManager.DataFile3);
			for(int i = 0; i < Files.Length; i++){
				if(Files[i].Contains("Assets/Chr")){
					if(Files[i].Contains(".ply") && Files[i].Contains(Name)){
						ValidFiles.Add(Path.GetFileNameWithoutExtension(Files[i]));
					}
				}
			}
			return ValidFiles.ToArray();
		}
		
		public static string[] GetModelFiles(string Name){
			List<string> ValidFiles = new List<string>();
			string[] Files = AssetManager.GetFileNames(AssetManager.DataFile3);
			for(int i = 0; i < Files.Length; i++){
				if(Files[i].Contains("Assets/Chr")){
					if(Files[i].Contains(".ply") && Files[i].Contains(Name)){
						ValidFiles.Add(Files[i]);
					}
				}
			}
			return ValidFiles.ToArray();
		}
	}
}
