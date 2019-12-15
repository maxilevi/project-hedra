/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 30/12/2016
 * Time: 05:46 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Hedra.AISystem;
using Hedra.AISystem.Humanoid;
using Hedra.Components;
using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.IO;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Networking;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Scripting;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.WorldBuilding;
using Hedra.Game;
using Hedra.Items;
using Hedra.Mission;
using System.Numerics;
using Hedra.Engine;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.EntitySystem;
using Hedra.Numerics;
using Silk.NET.Windowing.Common;

namespace Hedra.User
{
    /// <summary>
    /// Description of CommandManager.
    /// </summary>
    public static class CommandManager
    {
        
        public static bool ProcessCommand(string Command, IPlayer Caster, out string Result)
        {
            try
            {
                Result = string.Empty;
                Command = Command.Remove(0,1);
                string[] Parts = Command.Split(' ');
                switch (Parts[0].ToLowerInvariant())
                {
                    case "tp":
                    {

                        if (Parts[1] == "spawn")
                        {
                            Caster.Position = World.SpawnPoint;
                            
                        }
                        if (Parts[1] == "quest")
                        {
                            var quest = Caster.Questing.ActiveQuests[0];
                            Caster.Position = quest.Location;
                        }

                        if (Parts[1] == "spawnvillage")
                        {
                            Caster.Position = World.SpawnVillagePoint;
                        }

                        if (Parts[1] == "witchhut")
                        {
                            var structs = StructureHandler.GetNearStructures(Caster.Position);
                            for(var i = 0; i < structs.Length; ++i)
                                if (structs[i].WorldObject is WitchHut hut)
                                {
                                    Caster.Position = hut.StealPosition;
                                }
                        }
                        if (Parts[1] == "graveyard")
                        {
                            var structs = StructureHandler.GetNearStructures(Caster.Position);
                            for (var i = 0; i < structs.Length; ++i)
                            {
                                if (structs[i].WorldObject is Graveyard hut)
                                {
                                    Caster.Position = hut.Position;
                                }
                            }
                        }
                        if (Parts[1] == "merchant")
                        {
                            var structs = StructureHandler.GetNearStructures(Caster.Position);
                            for (var i = 0; i < structs.Length; ++i)
                            {
                                if (structs[i].WorldObject is TravellingMerchant hut)
                                {
                                    Caster.Position = hut.Position;
                                }
                            }
                        }
                        if (float.TryParse(Parts[1], out var x))
                        {
                            float.TryParse(Parts[2], out var y);
                            float.TryParse(Parts[3], out var z);
                            Caster.Position = new Vector3(x,y,z);
                        }
                        return true;
                    }
                    case "hideworld":
                    {
                        GameSettings.HideWorld = !GameSettings.HideWorld;
                        return true;
                    }
                    case "spit":
                    {
                        var proj = new ParticleProjectile(Caster, Caster.Position)
                        {
                            Propulsion = Caster.Orientation * 2f,
                            Color = Color.LawnGreen.ToVector4() * .85f,
                            UseLight = false
                        };
                        World.AddWorldObject(proj);
                        return true;
                    }
                    case "hurt":
                        Caster.SearchComponent<DamageComponent>().Immune = false;
                        Caster.Damage(float.Parse(Parts[1]), null, out float exp);
                        break;

                    case "debug":
                        GameSettings.DebugMode = !GameSettings.DebugMode;
                        break;

                    case "mana":
                        Caster.Mana = Caster.MaxMana;
                        break;

                    case "icon":
                        Caster.ShowIcon((CacheItem) Enum.Parse(typeof(CacheItem), Parts[1]));
                        break;
                    
                    case "cfg":
                    {
                        var variable = Parts[1];
                        var prop = typeof(GameSettings).GetProperty(variable, BindingFlags.Public | BindingFlags.Static);
                        if (Parts.Length > 2)
                        {
                            prop.SetValue(null, Convert.ChangeType(Parts[2], prop.PropertyType), null);
                        }
                        Result = $"{variable} = {prop.GetValue(null, null).ToString()}";
                        return true;
                    }
                    case "spawningEffect":
                        GameManager.SpawningEffect = true;
                        return true;
                    case "hide":
                        Caster.Model.Enabled = !Caster.Model.Enabled;
                        break;
  
                    case "bloom":
                        GameSettings.Bloom = !GameSettings.Bloom;
                        break;
                    case "kill":
                    {
                        if (Parts.Length == 1) Caster.Health = 0f;
                        return true;
                    }
                    case "logItems":
                    {
                        for (var i = 0; i < Caster.Inventory.Length; i++)
                        {
                            if(Caster.Inventory[i] != null)
                                Log.WriteLine($" {i} {Caster.Inventory[i]}");
                        }
                        return true;
                    }
                    case "rain":
                        SkyManager.Weather.IsRaining = true;
                        return true;
                    case "sunny":
                        SkyManager.Weather.IsRaining = false;
                        return true;
                    case "xp":
                        Caster.XP += float.Parse(Parts[1]);
                        return true;
                    case "list" when Parts[1] == "items":
                        Result = string.Join(Environment.NewLine, ItemLoader.Templater.Templates.Select(T => T.Name));
                        return true;
                }

                if (Parts[0] == "distfromvill")
                {
                    var dist = (Caster.Position - World.SpawnVillagePoint).Xz().LengthFast();
                    Result = $"Spawn village is '{dist}' meters away";
                    return true;
                }
                if (Parts[0] == "lvl")
                {
                    Caster.Level = int.Parse(Parts[1]);
                    return true;
                }

                if (Parts[0] == "clear")
                {
                    Caster.Chat.Clear();
                    return true;
                }
                if(Parts[0] == "highlight")
                {
                    World.HighlightArea(Caster.Position, new Vector4(1, 0, 0, 1), 32, 4f);
                    return true;
                }
                if(Parts[0] == "fly")
                {
                    GameManager.Player.Physics.UsePhysics = !GameManager.Player.Physics.UsePhysics;
                }
                if (Parts[0] == "villager")
                {
                    var vill = World.InRadius<Village>(Caster.Position, VillageDesign.MaxVillageRadius).FirstOrDefault();
                    Result = "Couldn't find any near village";
                    if (vill == null) return false;
                    var human = NPCCreator.SpawnHumanoid(HumanType.Bard, Caster.Position + Caster.Orientation * 16f);
                    human.AddComponent(new TalkComponent(human));
                    human.AddComponent(new RoamingVillagerAIComponent(human, vill.Graph));
                    Result = "Success";
                    return true;
                }

                if (Parts[0] == "resetquests")
                {
                    Caster.Questing.Empty();
                }
                if (Parts[0] == "quest")
                {
                    var position = Caster.Position + Caster.Orientation * 16f;
                    IMissionDesign quest;
                    if (Parts.Length == 2)
                    {
                        quest = MissionPool.Grab(Parts[1]);
                    }
                    else
                    {
                        quest = MissionPool.Random(position);
                    }

                    NPCCreator.SpawnQuestGiver(position, quest, Utils.Rng);
                    Result = "Success";
                    return true;
                }

                if (Parts[0] == "resetcooldowns")
                {
                    GameManager.Player.Toolbar.ResetCooldowns();
                }
                if (Parts[0] == "speed"){
                    Caster.Speed += float.Parse(Parts[1]);
                    return true;
                }
                if (Parts[0] == "attackspeed")
                {
                    Caster.AttackSpeed = float.Parse(Parts[1]);
                    return true;
                }
                if (Parts[0] == "dmg"){
                    Caster.AttackPower += float.Parse(Parts[1]);
                    return true;
                }
                if (Parts[0] == "drop")
                {
                    if (Parts[1] == "coin")
                    {
                        World.DropItem(ItemPool.Grab(ItemType.Gold), Caster.Position + Caster.Orientation * 16f);
                        return true;
                    }
                }
                
                if(Parts[0] == "sit")
                {
                    Caster.IsSitting = !Caster.IsSitting;
                    return true;
                }
                
                if(Parts[0] == "tie")
                {
                    Caster.IsTied = !Caster.IsTied;
                    return true;
                }
                if (Parts[0] == "spawnAnimation")
                {
                    Caster.PlaySpawningAnimation = true;
                    return true;
                }

                if (Parts[0] == "greet")
                {
                    Caster.Greet();
                    return true;
                }
                if (Parts[0] == "seed")
                {
                    Result = World.Seed.ToString();
                    return true;
                }
                if(Parts[0] == "get")
                {
                    if (Parts[1] == "attackspeed")
                    {
                        Result = Caster.AttackSpeed.ToString(CultureInfo.InvariantCulture);
                        return true;
                    }
                    if (Parts[1] == "recipe")
                    {
                        Caster.Inventory.AddItem(ItemPool.Grab(new ItemPoolSettings((ItemTier)Utils.Rng.Next(0, (int)ItemTier.Divine), EquipmentType.Recipe)));
                    }
                    if (Parts[1] == "item")
                    {
                        Caster.Inventory.AddItem(ItemPool.Grab(Parts[2]));
                    }
                    if (Parts[1] == "sword")
                    {
                        Caster.Inventory.AddItem(ItemPool.Grab(new ItemPoolSettings((ItemTier)Utils.Rng.Next(0, (int) ItemTier.Divine), EquipmentType.Sword)));
                    }
                    if (Parts[1] == "axe")
                    {
                        Caster.Inventory.AddItem(ItemPool.Grab(new ItemPoolSettings((ItemTier)Utils.Rng.Next(0, (int)ItemTier.Divine), EquipmentType.Axe)));
                    }
                    if (Parts[1] == "katar")
                    {
                        Caster.Inventory.AddItem(ItemPool.Grab(new ItemPoolSettings((ItemTier)Utils.Rng.Next(0, (int)ItemTier.Divine), EquipmentType.Katar)));
                    }
                    if (Parts[1] == "hammer")
                    {
                        Caster.Inventory.AddItem(ItemPool.Grab(new ItemPoolSettings((ItemTier)Utils.Rng.Next(0, (int)ItemTier.Divine), EquipmentType.Hammer)));
                    }
                    if (Parts[1] == "claw")
                    {
                        Caster.Inventory.AddItem(ItemPool.Grab(new ItemPoolSettings((ItemTier)Utils.Rng.Next(0, (int)ItemTier.Divine), EquipmentType.Claw)));
                    }
                    if (Parts[1] == "blades")
                    {
                        Caster.Inventory.AddItem(ItemPool.Grab(new ItemPoolSettings((ItemTier)Utils.Rng.Next(0, (int)ItemTier.Divine), EquipmentType.DoubleBlades)));
                    }
                    if (Parts[1] == "bow")
                    {
                        Caster.Inventory.AddItem(ItemPool.Grab(new ItemPoolSettings((ItemTier)Utils.Rng.Next(0, (int)ItemTier.Divine), EquipmentType.Bow)));
                    }
                    if (Parts[1] == "knife")
                    {
                        Caster.Inventory.AddItem(ItemPool.Grab(new ItemPoolSettings((ItemTier)Utils.Rng.Next(0, (int)ItemTier.Divine), EquipmentType.Knife)));
                    }
                    if (Parts[1] == "staff")
                    {
                        Caster.Inventory.AddItem(ItemPool.Grab(new ItemPoolSettings((ItemTier)Utils.Rng.Next(0, (int)ItemTier.Divine), EquipmentType.Staff)));
                    }
                    if (Parts[1] == "ring")
                    {
                        Caster.Inventory.AddItem(ItemPool.Grab(new ItemPoolSettings((ItemTier)Utils.Rng.Next(0, (int)ItemTier.Divine), EquipmentType.Ring)));
                    }
                    if (Parts[1] == "glider")
                    {
                        Caster.Inventory.AddItem(ItemPool.Grab(ItemType.Glider));
                    }
                    if (Parts[1] == "boat")
                    {
                        Caster.Inventory.AddItem(ItemPool.Grab(ItemType.Boat));
                    }
                    if (Parts[1] == "gold")
                    {
                        var item = ItemPool.Grab(ItemType.Gold);
                        item.SetAttribute(CommonAttributes.Amount, int.Parse(Parts[2]));
                        Caster.Inventory.AddItem(item);
                    }
                    Result = $"Giving item {Parts[1].ToUpperInvariant()} to {Caster.Name}";
                    return true;
                }
                if(Parts[0] == "time"){
                    if(Parts[1] == "speed")
                    {
                        SkyManager.DaytimeSpeed = int.Parse(Parts[2]);
                        return true;
                    }
                    SkyManager.SetTime(int.Parse(Parts[1]));
                    return true;
                }

                if (Parts[0] == "plaque")
                {
                    Caster.MessageDispatcher.ShowPlaque(Parts[1], 3);
                }
                if (Parts[0] == "poison")
                {
                    Caster.AddComponent(new PoisonComponent(Caster, null, 5f, 30f));
                }
                if (Parts[0] == "freeze")
                {
                    Caster.AddComponent(new FreezingComponent(Caster, null, 5f, 30f));
                }
                if (Parts[0] == "bleed")
                {
                    Caster.AddComponent(new BleedingComponent(Caster, null, 5f, 30f));
                }
                if (Parts[0] == "host")
                {
                    Network.Instance.Host();
                }
                if (Parts[0] == "slow")
                {
                    Caster.AddComponent(new SlowingComponent(Caster, null, 5f, 30f));
                }
                if (Parts[0] == "fast")
                {
                    Caster.AddComponent(new SpeedComponent(Caster));
                }
                if (Parts[0] == "burn")
                {
                    Caster.AddComponent(new BurningComponent(Caster, null, 5f, 30f));
                }
                if (Parts[0] == "knock")
                {
                    Caster.KnockForSeconds(float.Parse(Parts[1]));
                }

                if (Parts[0] == "maximize")
                {
                    Program.GameWindow.WindowState = WindowState.Maximized;
                }
                if (Parts[0] == "audiotest")
                {
                    for (int i = 0; i < 16; i++)
                    {
                        World.SpawnMob(MobType.Boar, Caster.Position + Caster.Orientation * 16f, Utils.Rng);
                    }
                    return true;
                }
                if (Parts[0] == "hide")
                {
                    LocalPlayer.Instance.Enabled = !LocalPlayer.Instance.Enabled;
                    return true;
                }
                if (Parts[0] == "opendoor")
                {
                    var structures = StructureHandler.GetNearStructures(Caster.Position);
                    for (var i = 0; i < structures.Length; ++i)
                    {
                        structures[i].WorldObject.Search<Door>().Where(D => Caster.Distance(D.Position) < 16).ToList().ForEach(D => D.IsLocked = false);
                    }
                }
                if (Parts[0] == "spawn")
                {
                    if(Parts[1] == "escape")
                    {
                        var vill = NPCCreator.SpawnVillager(Caster.Position + Caster.Orientation * 16, Utils.Rng);
                        vill.AddComponent(new EscapeAIComponent(vill, Caster));
                        return true;
                    }

                    if (Parts[1] == "follow")
                    {
                        var vill = NPCCreator.SpawnVillager(Caster.Position + Caster.Orientation * 16, Utils.Rng);
                        vill.AddComponent(new FollowAIComponent(vill, Caster));
                        return true;
                    }{}

                    if(Parts[1] == "bandit")
                    {
                        NPCCreator.SpawnBandit(Caster.Position + Caster.Orientation * 32, Caster.Level, BanditOptions.Default);
                        return true;
                    }
                    if(Parts[1] == "undead")
                    {
                        NPCCreator.SpawnBandit(Caster.Position + Caster.Orientation * 32, Caster.Level, new BanditOptions
                        {
                            ModelType = Utils.Rng.NextBool() ? HumanType.VillagerGhost : HumanType.BeasthunterSpirit
                        });
                        return true;
                    }
                    if(Parts[1] == "plantling")
                    {
                        NPCCreator.SpawnHumanoid(HumanType.Mandragora, Caster.Position + Caster.Orientation * 32);
                        return true;
                    }
                    if(Parts[1] == "merchant"){
                        NPCCreator.SpawnHumanoid(HumanType.TravellingMerchant, Caster.Position + Caster.Orientation * 32);
                        return true;
                    }
                    if(Parts[1] == "explorers")
                    {
                        TravellingExplorers.Build(Caster.Position + Caster.Orientation * 32, Utils.Rng);
                        return true;
                    }
                    if(Parts[1] == "abadonedexplorer")
                    {
                        TravellingExplorers.BuildAbandonedExplorerWithQuest(Caster.Position + Caster.Orientation * 32, Utils.Rng);
                        return true;
                    }
                    if (World.MobFactory.ContainsFactory(Parts[1]))
                    {
                        var amount = Parts.Length > 2 ? int.Parse(Parts[2]) : 1;
                        for (var i = 0; i < amount; i++)
                        {
                            World.SpawnMob(Parts[1], Caster.Position + Caster.Orientation * 32, Utils.Rng);
                        }
                    }
                    else
                    {
                        NPCCreator.SpawnHumanoid(Parts[1], Caster.Position + Caster.Orientation * 32);
                    }
                    return true;
                }
                if(Parts[0] == "chest")
                {
                    World.SpawnChest(Caster.Position + Caster.Orientation * 32, ItemPool.Grab(Parts[1]) );
                    return true;
                }

                if (Parts[0] == "watch")
                {
                    GameSettings.WatchScriptChanges = !GameSettings.WatchScriptChanges;
                    Result = $"Watching script changes is now: {(GameSettings.WatchScriptChanges ? "ON" : "OFF")}";
                    return true;
                }
                if (Parts[0] == "exec")
                {
                    if (Parts.Length != 2)
                    {
                        Result = "Invalid arguments";
                        return false;
                    }
                    Interpreter.GetFunction(Parts[1], Parts[2]).Invoke();
                    return true;
                }

                if (Parts[0] == "reload")
                {
                    Interpreter.Reload();
                    return true;
                }

                if (Parts[0] == "sethitbox")
                {
                    LocalPlayer.Instance.Model = LocalPlayer.Instance.Model;
                }

                if (Parts[0] == "debai")
                {
                    GameSettings.DebugAI = !GameSettings.DebugAI;
                }

                if (Parts[0] == "debpath")
                {
                    GameSettings.DebugNavMesh = !GameSettings.DebugNavMesh;
                }

                if (Parts[0] == "fisherman")
                {
                    var fisherman = NPCCreator.SpawnHumanoid(HumanType.Fisherman, Caster.Position + Caster.Orientation * 32);
                    //fisherman.RemoveComponent(fisherman.SearchComponent<BasicAIComponent>());
                    fisherman.AddComponent(new FishermanAIComponent(fisherman, (Caster.Position + Caster.Orientation * 32).Xz(), Vector2.One * 64f));
                }

                if (Parts[0] == "frustum")
                {
                    GameSettings.DebugFrustum = !GameSettings.DebugFrustum;
                }
                if (Parts[0] == "realm")
                {

                    if (Parts[1] == "ghosttown")
                        Caster.Realms.GoTo(RealmHandler.GhostTown);
                    else
                        Caster.Realms.GoTo(int.Parse(Parts[1]));

                }

                if (Parts[0] == "discard")
                {
                    World.Discard();
                    lock (World.Chunks)
                    {
                        var count = World.Chunks.Count;
                        for (int i = count-1; i > -1; i--)
                        {
                            World.RemoveChunk(World.Chunks[i]);
                        }
                    }
                    World.StructureHandler.Discard();
                    Engine.StructureSystem.StructureHandler.CheckStructures(World.ToChunkSpace(Caster.Position));
                }

                if (Parts[0] == "place")
                {
                    if (Parts[1] == "water")
                    {
                        var chunk = World.GetChunkAt(Caster.Position);
                        var blockspace = World.ToBlockSpace(Caster.Position);
                        chunk?.SetBlockAt((int) blockspace.X, (int) blockspace.Y, (int) blockspace.Z,
                            BlockType.Water);
                    }
                }
                if (Parts[0] == "regenerate")
                {
                    World.RemoveChunk(World.GetChunkAt(Caster.Position));
                }
                if (Parts[0] == "printch")
                {
                    World.GetChunkAt(Caster.Position).Test();
                }

                if (Parts[0] == "automata")
                {
                    if (Parts.Length == 2 && Parts[1] == "all")
                    {
                        var chunks = World.Chunks;
                        var count = World.Chunks.Count;
                        for (var i = count-1; i > -1; i--)
                        {
                            if(chunks[i].NeighboursExist)
                                chunks[i].Automatons.Update();
                            //World.AddChunkToQueue(chunks[i], ChunkQueueType.Mesh);
                        }
                    }
                    else
                    {
                        var chunk = World.GetChunkAt(Caster.Position);
                        chunk.Automatons.Update();
                        World.AddChunkToQueue(chunk, ChunkQueueType.Mesh);
                    }
                }
                if (Parts[0] == "rebuild")
                {
                    lock (World.Chunks)
                    {
                        World.Builder.ResetMeshProfile();
                        var count = World.Chunks.Count;
                        for (var i = count-1; i > -1; i--)
                        {
                            World.AddChunkToQueue(World.Chunks[i], ChunkQueueType.Mesh);
                        }
                    }
                }
                Result = "Unknown command.";
                Log.WriteLine("Unknown command.");
            }
            catch(Exception e)
            {
                Log.WriteLine(e.ToString());
                Result = $"Command failed.{Environment.NewLine}{e.Message}";
                return false;
            }
            return false;
        }
    }
}
