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
using Hedra.AISystem.Humanoid;
using Hedra.Components;
using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using OpenTK;
using Hedra.Engine.Player;
using Hedra.Engine.Generation;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Game;
using Hedra.Engine.IO;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Networking;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.Scripting;
using Hedra.Engine.Sound;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.WorldBuilding;
using Hedra.Game;
using Hedra.Items;
using Hedra.Sound;

namespace Hedra.Engine.Management
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
                            Caster.BlockPosition = World.SpawnPoint;
                            
                        }

                        if (Parts[1] == "spawnvillage")
                        {
                            Caster.BlockPosition = World.SpawnVillagePoint;
                        }
                        if (float.TryParse(Parts[1], out var x))
                        {
                            float.TryParse(Parts[2], out var y);
                            float.TryParse(Parts[3], out var z);
                            Caster.BlockPosition = new Vector3(x,y,z);
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
                    var dist = (Caster.Position - World.SpawnVillagePoint).Xz.LengthFast;
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
                    var human = World.WorldBuilding.SpawnHumanoid(HumanType.Warrior, Caster.Position + Caster.Orientation * 16f);
                    human.AddComponent(new TalkComponent(human));
                    human.AddComponent(new RoamingVillagerAIComponent(human, vill.Graph));
                    Result = "Success";
                    return true;
                }
                
                if (Parts[0] == "quest")
                {
                    var position = Caster.Position + Caster.Orientation * 16f;
                    var human = World.WorldBuilding.SpawnVillager(position, Utils.Rng);
                    var tier = Parts.Length == 2 ? (QuestTier) Enum.Parse(typeof(QuestTier), Parts[1], true) : QuestTier.Any;
                    human.AddComponent(
                        new QuestGiverComponent(human, QuestPool.Grab(Quests.FindOverworldStructure).Build(position, Utils.Rng, human))
                    );
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
                if (Parts[0] == "spawn")
                {
                    if(Parts[1] == "bandit")
                    {
                        World.WorldBuilding.SpawnBandit(Caster.Position + Caster.Orientation * 32, Caster.Level, false);
                        return true;
                    }
                    if(Parts[1] == "plantling")
                    {
                        World.WorldBuilding.SpawnHumanoid(HumanType.Mandragora, Caster.Position + Caster.Orientation * 32);
                        return true;
                    }
                    if(Parts[1] == "merchant"){
                        World.WorldBuilding.SpawnHumanoid(HumanType.TravellingMerchant, Caster.Position + Caster.Orientation * 32);
                        return true;
                    }
                    if(Parts[1] == "explorers")
                    {
                        TravellingExplorers.Build(Caster.Position + Caster.Orientation * 32, Utils.Rng);
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
                        World.WorldBuilding.SpawnHumanoid(Parts[1], Caster.Position + Caster.Orientation * 32);
                    }
                    return true;
                }
                if(Parts[0] == "chest")
                {
                    World.SpawnChest(Caster.Position + Caster.Orientation * 32, ItemPool.Grab(Parts[1]) );
                    return true;
                }
                if (Parts[0] == "exec")
                {    
                    Interpreter.GetFunction(Parts[1], Parts[2])();
                }
                if (Parts[0] == "realm")
                {

                    if (Parts[1] == "ghosttown")
                        Caster.Realms.GoTo(RealmHandler.GhostTown);
                    else
                        Caster.Realms.GoTo(int.Parse(Parts[1]));

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
