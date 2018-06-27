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
		public const float SaveVersion = 1.12f;
		
		public static void SavePlayer(PlayerInformation Player)
        {
			var chrFile = AssetManager.AppData+"/Characters/"+Player.Name;
			
			if(File.Exists(chrFile + ".db"))
            {
                if (File.Exists(chrFile + ".db.bak"))
                {
                    File.Delete(chrFile + ".db.bak");
                }
                File.Copy(chrFile + ".db", chrFile + ".db.bak");
				
				var fInfo = new FileInfo(chrFile + ".db.bak");
				fInfo.Attributes |= FileAttributes.Hidden;
			}
            using (var fs = File.Create(AssetManager.AppData + "/Characters/" + Player.Name + ".db"))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(SaveVersion);
                    bw.Write(Player.Name);
                    bw.Write(Player.BlockPosition.X);
                    bw.Write(Player.BlockPosition.Y + 4);
                    bw.Write(Player.BlockPosition.Z);

                    bw.Write(Player.Rotation.X);
                    bw.Write(Player.Rotation.Y);
                    bw.Write(Player.Rotation.Z);

                    bw.Write(Player.Health);

                    bw.Write(Player.Xp);
                    bw.Write(Player.Level);

                    bw.Write(Player.Mana);

                    bw.Write(Player.WorldSeed);

                    bw.Write(Player.AbilityTreeArray.Length);
                    bw.Write(Player.AbilityTreeArray);

                    bw.Write(Player.ToolbarArray.Length);
                    bw.Write(Player.ToolbarArray);

                    bw.Write(Player.TargetPosition);

                    bw.Write(Player.Daytime);
                    bw.Write(Player.Class.Name);
                    bw.Write(Player.RandomFactor);

                    var items = Player.Items;
                    if (items != null)
                    {
                        bw.Write(items.Length);
                        foreach (var pair in items)
                        {
                            bw.Write(pair.Key);
                            var itemBytes = pair.Value.ToArray();
                            bw.Write(itemBytes.Length);
                            bw.Write(itemBytes);
                        }
                    }
                }
            }
		}
	
		public static PlayerInformation DataFromPlayer(LocalPlayer Player)
        {
		    var data = new PlayerInformation
		    {
		        Level = Player.Level,
		        Health = Player.Health,
                Mana = Player.MaxXP,
		        Xp = Player.XP,
		        WorldSeed = World.Seed,
		        Name = Player.Name,
		        Rotation = Player.Rotation,
		        BlockPosition = Player.BlockPosition,
		        AbilityTreeArray = Player.AbilityTree.ToArray(),
		        ToolbarArray = Player.Toolbar.ToArray(),
		        TargetPosition = Player.Physics.TargetPosition,
		        Daytime = EnvironmentSystem.SkyManager.DayTime,
		        Class = Player.Class,
		        RandomFactor = Player.RandomFactor,
		        Items = Player.Inventory.ToArray()
		    };

		    return data;
		}
		
		public static PlayerInformation LoadPlayer(string FileName)
		{
		    var info = new FileInfo(FileName);
		    FileName = info.Length == 0 ? $"{FileName}.bak" : FileName;
            var data = DataManager.LoadPlayer(File.Open(FileName, FileMode.Open));
			if(data.IsCorrupt)
            {
                Log.WriteLine($"[IO] Detected corrupt character file '{Path.GetFileName(FileName)}'.");
			    if (!File.Exists($"{FileName}.bak"))
			    {
			        Log.WriteLine($"[IO] Failed to load character '{Path.GetFileName(FileName)}'.");
                    return null;
			    }
                Log.WriteLine("[IO] Character backup found! Retrying...");
                File.Delete(FileName);
			    File.Copy($"{FileName}.bak", FileName);
			    var fInfo = new FileInfo(FileName)
			    {
			        Attributes = FileAttributes.Normal
			    };
                return LoadPlayer($"{FileName}.bak");
            }		
			return data;
		}
		
		public static PlayerInformation LoadPlayer(Stream Str)
        {
			var information = new PlayerInformation();
			Dictionary<int, Item> items;
            using (var br = new BinaryReader(Str))
            {
                float version = br.ReadSingle();
                if (version < 1.0f) return null;
                information.Name = br.ReadString();
                information.BlockPosition = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                information.Rotation = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                if (version < 1.1f) br.ReadSingle();
                if (version < 1.12f) br.ReadSingle();
                information.Health = br.ReadSingle();
                information.Xp = br.ReadSingle();
                information.Level = br.ReadInt32();
                information.Mana = br.ReadSingle();
                information.WorldSeed = br.ReadInt32();
                information.AbilityTreeArray = br.ReadBytes(br.ReadInt32());
                information.ToolbarArray = br.ReadBytes(br.ReadInt32());
                information.TargetPosition = br.ReadVector3();
                information.Daytime = br.ReadSingle();
                information.Class = ClassDesign.FromString(version < 1.11f ? intToClassDesignString(br.ReadInt32()) : br.ReadString());
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

		public static PlayerInformation[] PlayerFiles
        {
			get
            {
				var filesList = new List<PlayerInformation>();
				string[] files = Directory.GetFiles(AssetManager.AppData + "Characters/");

				for(var i = 0; i < files.Length; i++)
                {
					if(files[i].EndsWith(".bak")) 
						continue;
					try
					{
					    var information = LoadPlayer(files[i]);
					    if (information == null)
					    {
					        File.Delete(files[i]);
					        if (File.Exists(files[i] + ".bak"))
					        {
					            File.Delete(files[i] + ".bak");
					        }
                            continue;
					    }
                        filesList.Add( information );
					}
                    catch (Exception e)
                    {
						if(e is UnauthorizedAccessException)
                        {
							Log.WriteLine("Unathoriazed access in file: " + Path.GetFileName(files[i]));
							continue;
						}
						Log.WriteLine(e.ToString());
					}
				}
				return filesList.ToArray();
			}
		}
		
		
		public static int CharacterCount
        {
			get
            { 
				string[] files = Directory.GetFiles(AssetManager.AppData+"Characters/");
				var count = 0;
				for(var i = 0; i < files.Length; i++){
					if(!files[i].EndsWith(".bak")) count++;
				}
				return count;
			}
		}
	}
}
