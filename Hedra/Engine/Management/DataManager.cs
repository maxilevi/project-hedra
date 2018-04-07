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
using Hedra.Engine.ClassSystem;
using Hedra.Engine.ItemSystem;

namespace Hedra.Engine.Management
{
	/// <summary>
	/// Description of SaveManager.
	/// </summary>
	public static class DataManager
	{
		public const float SaveVersion = 1.11f;
		
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
					
					Bw.Write(Player.AddonHealth);
					Bw.Write(Player.Health);
					
					Bw.Write(Player.Xp);			
					Bw.Write(Player.Level);
					
					Bw.Write(Player.Mana);
					
					Bw.Write(Player.WorldSeed);			
					
					Bw.Write(Player.AbilityTreeArray.Length);
					Bw.Write(Player.AbilityTreeArray);
					
					Bw.Write(Player.ToolbarArray.Length);
					Bw.Write(Player.ToolbarArray);
					
					Bw.Write(Player.TargetPosition);
					
					Bw.Write(Player.Daytime);
					Bw.Write(Player.Class.Name);
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
		    var data = new PlayerInformation
		    {
		        Level = Player.Level,
		        Health = Player.Health,
		        AddonHealth = Player.AddonHealth,
                Mana = Player.MaxXP,
		        Xp = Player.XP,
		        WorldSeed = World.Seed,
		        Name = Player.Name,
		        Rotation = Player.Rotation,
		        BlockPosition = Player.BlockPosition,
		        AbilityTreeArray = Player.AbilityTree.ToArray(),
		        ToolbarArray = Player.Toolbar.ToArray(),
		        TargetPosition = Player.Physics.TargetPosition,
		        Daytime = Enviroment.SkyManager.DayTime,
		        Class = Player.Class,
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
			using(var br = new BinaryReader(Str)){
				float version = br.ReadSingle();
			    if (version < 1.0f) return null;
				information.Name = br.ReadString();
				information.BlockPosition = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
				information.Rotation = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
				if(version < 1.1f) br.ReadSingle();
                information.AddonHealth = br.ReadSingle();
				information.Health = br.ReadSingle();
				information.Xp = br.ReadSingle();
				information.Level = br.ReadInt32();		
				information.Mana = br.ReadSingle();
				information.WorldSeed = br.ReadInt32();
			    information.AbilityTreeArray = br.ReadBytes(br.ReadInt32());
				information.ToolbarArray = br.ReadBytes(br.ReadInt32());
				information.TargetPosition = br.ReadVector3();
				information.Daytime = br.ReadSingle();
				information.Class = ClassDesign.FromString( version < 1.11f ? intToClassDesignString(br.ReadInt32()) : br.ReadString() );
                information.RandomFactor = br.ReadSingle();
                items = new Dictionary<int, Item>();
			    int itemCount = br.ReadInt32();
			    for (var i = 0; i < itemCount; i++)
			    {
			        var index = br.ReadInt32();
			        var item = Item.FromArray(br.ReadBytes(br.ReadInt32()));
                    items.Add(index, item);
			    }
			}

			information.Items = items.ToArray();
			Str.Close();
			Str.Dispose();
			return information;
		}

        [Obsolete]
	    private static string intToClassDesignString(int OldClass)
        {
            var map = new []{"None", "Archer", "Rogue", "Warrior"};
            return map[OldClass];
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
