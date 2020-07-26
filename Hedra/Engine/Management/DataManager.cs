/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 15/06/2016
 * Time: 05:36 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Numerics;
using System.IO;
using Hedra.Engine.Player;
using Hedra.Engine.Generation;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.IO;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering.UI;
using Hedra.Items;
using Hedra.Mission;

namespace Hedra.Engine.Management
{
    /// <summary>
    /// Description of SaveManager.
    /// </summary>
    public static class DataManager
    {
        public static string CharactersFolder => $"{AssetManager.AppPath}/Characters/";
        private const float SaveVersion = 1.6f;
        
        public static void SavePlayer(PlayerInformation Information)
        {
            var chrFile = $"{CharactersFolder}{Information.Name}";
            
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
            using (var fs = File.Create(CharactersFolder + Information.Name + ".db"))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(SaveVersion);
                    bw.Write(Information.Name);

                    bw.Write(Information.Rotation.X);
                    bw.Write(Information.Rotation.Y);
                    bw.Write(Information.Rotation.Z);

                    bw.Write(Information.Health);

                    bw.Write(Information.Xp);
                    bw.Write(Information.Level);

                    bw.Write(Information.Mana);

                    bw.Write(Information.SkillsData.Length);
                    bw.Write(Information.SkillsData);
                    
                    bw.Write(Information.RealmData.Length);
                    bw.Write(Information.RealmData);

                    bw.Write(Information.ToolbarData.Length);
                    bw.Write(Information.ToolbarData);

                    bw.Write(Information.Class.Name);
                    bw.Write(Information.RandomFactor);

                    var items = Information.Items;
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
                    var recipes = Information.Recipes;
                    if (recipes != null)
                    {
                        bw.Write(recipes.Length);
                        for(var i = 0; i < recipes.Length; ++i)
                        {
                            bw.Write(recipes[i]);
                        } 
                    }
                    var quests = Information.Quests;
                    if (quests != null)
                    {
                        bw.Write(quests.Length);
                        for(var i = 0; i < quests.Length; ++i)
                        {
                            var questBytes = quests[i].ToArray();
                            bw.Write(questBytes.Length);
                            bw.Write(questBytes);
                        } 
                    }
                }
            }
        }
    
        public static PlayerInformation DataFromPlayer(IPlayer Player)
        {
            var data = new PlayerInformation
            {
                Level = Player.Level,
                Health = Player.Health,
                Mana = Player.Mana,
                Xp = Player.XP,
                Name = Player.Name,
                Rotation = Player.Rotation,
                Class = Player.Class,
                RandomFactor = Player.RandomFactor,
                Items = Player.Inventory.ToArray(),
                Recipes = Player.Crafting.RecipeNames,
                ToolbarData = Player.Toolbar.Serialize(),
                Quests = Player.Questing.GetSerializedQuests(),
                SkillsData = Player.AbilityTree.Serialize(),
                RealmData = Player.Realms.Serialize()
            };

            return data;
        }
        
        public static PlayerInformation LoadPlayer(string FileName)
        {
            var info = new FileInfo(FileName);
            FileName = info.Length == 0 ? $"{FileName}.bak" : FileName;
            var data = DataManager.LoadPlayer(File.Open(FileName, FileMode.Open));
            if (data == null)
            {
                Log.WriteLine($"Failed to load character {FileName}. It might be incompatible.");
            }
            else if(data.IsCorrupt)
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

        private static PlayerInformation LoadPlayer(Stream Str)
        {
            var information = new PlayerInformation();
            using (var br = new BinaryReader(Str))
            {
                float version = br.ReadSingle();
                if (version < 1.6f) return null;
                information.Name = br.ReadString();
                information.Rotation = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                information.Health = br.ReadSingle();
                information.Xp = br.ReadSingle();
                information.Level = br.ReadInt32();
                information.Mana = br.ReadSingle();
                
                information.SkillsData = br.ReadBytes(br.ReadInt32());
                information.RealmData = br.ReadBytes(br.ReadInt32()); 
                information.ToolbarData = br.ReadBytes(br.ReadInt32());
                
                information.Class = ClassDesign.FromString(br.ReadString());
                information.RandomFactor = br.ReadSingle();

                information.Items = LoadItems(br);
                information.Recipes = LoadRecipes(br);
                information.Quests = LoadQuests(br);
            }
            Str.Close();
            Str.Dispose();
            return information;
        }

        private static SerializedQuest[] LoadQuests(BinaryReader Reader)
        {
            var quests = new List<SerializedQuest>();
            var length = Reader.ReadInt32();
            for(var i = 0; i < length; ++i)
            {
                var quest = SerializedQuest.FromArray(Reader.ReadBytes(Reader.ReadInt32()));
                if(MissionPool.Exists(quest.Name) || quest.IsMetadata)
                    quests.Add(quest);
                else
                    Log.WriteLine($"Found non-existent quest design '{quest.Name}', removing...");
            }
            return quests.ToArray();
        }
        
        private static string[] LoadRecipes(BinaryReader Reader)
        {
            var recipes = new List<string>();
            var length = Reader.ReadInt32();
            for(var i = 0; i < length; ++i)
            {
                var name = Reader.ReadString();
                if(ItemPool.Exists(name))
                    recipes.Add(name);
                else
                    Log.WriteLine($"Found non-existent recipe '{name},' removing...");
            }

            return recipes.ToArray();
        }
        
        private static KeyValuePair<int, Item>[] LoadItems(BinaryReader Reader)
        {
            var items = new Dictionary<int, Item>();
            var itemCount = Reader.ReadInt32();
            for (var i = 0; i < itemCount; i++)
            {
                var index = Reader.ReadInt32();
                var item = Item.FromArray(Reader.ReadBytes(Reader.ReadInt32()));
                if (item != null)
                {
                    items.Add(index, item);
                }
                else
                {
                    Log.WriteLine($"Found non-existent item, removing...");
                }
            }
            return items.ToArray();
        }
        
        
        public static void DeleteCharacter(PlayerInformation Information)
        {
            File.Delete($"{CharactersFolder}{Information.Name}.db");
            File.Delete($"{CharactersFolder}{Information.Name}.db.bak");
        }

        public static PlayerInformation[] PlayerFiles
        {
            get
            {
                var filesList = new List<PlayerInformation>();
                string[] files = Directory.GetFiles(CharactersFolder);

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
                            Log.WriteLine("Unauthorized access in file: " + Path.GetFileName(files[i]));
                            continue;
                        }
                        Log.WriteLine(e.ToString());
                    }
                }
                return filesList.ToArray();
            }
        }
        
        
        public static int CharacterCount => PlayerFiles.Length;
    }
}
