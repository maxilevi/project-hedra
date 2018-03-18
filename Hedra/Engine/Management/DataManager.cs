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
using System.Collections.Generic;
using Hedra.Engine.ItemSystem;

namespace Hedra.Engine.Management
{
	/// <summary>
	/// Description of SaveManager.
	/// </summary>
	public static class DataManager
	{
		public const float SaveVersion = 1.0f;
		
		public static void SavePlayer(PlayerInformation Player){
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
					
					Bw.Write(Player.AddonHealth);
					Bw.Write(Player.Health);
					
					Bw.Write(Player.Xp);			
					Bw.Write(Player.Level);
					
					Bw.Write(Player.Mana);
					
					Bw.Write(Player.WorldSeed);			
					
					Bw.Write(Player.SkillsData.Length);
					Bw.Write(Player.SkillsData);
					
					Bw.Write(Player.SkillIDs.Length);
					Bw.Write(Player.SkillIDs);
					
					Bw.Write(Player.TargetPosition);
					
					Bw.Write(Player.Daytime);
					Bw.Write( (int) Player.ClassType);
                    Bw.Write(Player.RandomFactor);

				    var items = Player.Items;
                    if (items != null){
						Bw.Write(items.Length);
					    foreach (var pair in items)
					    {
					        Bw.Write(pair.Key);
					        var itemBytes = pair.Value.ToArray();
                            Bw.Write(itemBytes.Length);
                            Bw.Write(itemBytes);
					    }
					}
				}
			}
		}
	
		public static PlayerInformation DataFromPlayer(LocalPlayer Player){
			bool connected = Networking.NetworkManager.IsConnected;
		    var data = new PlayerInformation
		    {
		        Level = Player.Level,
		        Health = Player.Health,
		        Mana = Player.MaxXP,
		        Xp = Player.XP,
		        WorldSeed = connected ? Scenes.SceneManager.Game.CurrentInformation.WorldSeed : World.Seed,
		        Name = Player.Name,
		        Rotation = Player.Rotation,
		        BlockPosition = connected ? Scenes.SceneManager.Game.CurrentInformation.BlockPosition : Player.BlockPosition,
		        AddonHealth = Player.AddonHealth,
		        SkillsData = Player.SkillSystem.Save(),
		        SkillIDs = Player.Skills.Save(),
		        TargetPosition = Player.Physics.TargetPosition,
		        Daytime = connected ? Scenes.SceneManager.Game.CurrentInformation.Daytime : Enviroment.SkyManager.DayTime,
		        ClassType = Player.ClassType,
		        RandomFactor = Player.RandomFactor,
		        Items = Player.Inventory.ToArray()
		    };

		    return data;
		}
		
		public static PlayerInformation LoadPlayer(string FileName){
			
			var chrInfo = new FileInfo(FileName);
			if(chrInfo.Length == 0){
			    if (!File.Exists(FileName + ".bak")) return DataManager.LoadPlayer(File.Open(FileName, FileMode.Open));
			    File.Delete(FileName);
			    File.Copy(FileName+".bak", FileName);
			    var fInfo = new FileInfo(FileName)
			    {
			        Attributes = FileAttributes.Normal
			    };
			}
			
			return DataManager.LoadPlayer(File.Open(FileName, FileMode.Open));
		}
		
		public static PlayerInformation LoadPlayer(Stream Str){
			var information = new PlayerInformation();
			Dictionary<int, Item> items;
			using(var Br = new BinaryReader(Str)){
				float version = Br.ReadSingle();
			    if (version < 1.0f) return null;
				information.Name = Br.ReadString();
				information.BlockPosition = new Vector3(Br.ReadSingle(), Br.ReadSingle(), Br.ReadSingle());
				information.Rotation = new Vector3(Br.ReadSingle(), Br.ReadSingle(), Br.ReadSingle());
				information.Speed = Br.ReadSingle();
                information.AddonHealth = Br.ReadSingle();
				information.Health = Br.ReadSingle();
				information.Xp = Br.ReadSingle();
				information.Level = Br.ReadInt32();		
				information.Mana = Br.ReadSingle();
				information.WorldSeed = Br.ReadInt32();
			    information.SkillsData = Br.ReadBytes(Br.ReadInt32());
				information.SkillIDs = Br.ReadBytes(Br.ReadInt32());
				information.TargetPosition = Br.ReadVector3();
				information.Daytime = Br.ReadSingle();
				information.ClassType = (Class) Br.ReadInt32();
                information.RandomFactor = Br.ReadSingle();
                items = new Dictionary<int, Item>();
			    int itemCount = Br.ReadInt32();
			    for (var i = 0; i < itemCount; i++)
			    {
			        var index = Br.ReadInt32();
			        var item = Item.FromArray(Br.ReadBytes(Br.ReadInt32()));
                    items.Add(index, item);
			    }
			}

			information.Items = items.ToArray();
			Str.Close();
			Str.Dispose();
			return information;
		}		

		public static PlayerInformation[] PlayerFiles{
			get{
				var filesList = new List<PlayerInformation>();
				string[] files = Directory.GetFiles(AssetManager.AppData+"Characters/");

				for(int i = 0; i < files.Length; i++){
					if(files[i].EndsWith(".bak")) 
						continue;
					try
					{
					    var information = LoadPlayer(files[i]);
					    if (information == null)
					    {
					        File.Delete(files[i]);
                            if(File.Exists(files[i] + ".bak")) File.Delete(files[i] + ".bak");
                            continue;
					    }
                        filesList.Add( information );
					}catch(Exception e){
						if(e is UnauthorizedAccessException){
							Log.WriteLine("Unathoriazed access in file: "+Path.GetFileName(files[i]));
							continue;
						}
						Log.WriteLine(e.ToString());
					}
				}
				return filesList.ToArray();
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
