﻿/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 30/12/2016
 * Time: 05:46 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Globalization;
using OpenTK;
using Hedra.Engine.Player;
using Hedra.Engine.Generation;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Sound;

namespace Hedra.Engine.Management
{
	/// <summary>
	/// Description of CommandManager.
	/// </summary>
	public static class CommandManager
	{
		
		public static bool ProcessCommand(string Command, Entity Caster, out string Result){
			try
			{
			    Result = string.Empty;
				Command = Command.Remove(0,1);
				string[] Parts = Command.Split(' ');
				if(Parts[0] == "tp"){
					if(Parts[1] == "obj") Caster.BlockPosition = World.QuestManager.Quest.IconPosition - Vector3.One.Xz.ToVector3() * 80;
					if(Parts[1] == "merchant"){
						if(World.StructureGenerator.MerchantPosition != Vector3.Zero)
							Caster.BlockPosition = World.StructureGenerator.MerchantPosition - Vector3.One.Xz.ToVector3() * 8;
						else 
							return false;
					}
					float x, y, z;
					if(float.TryParse(Parts[1], out x)){
					   	float.TryParse(Parts[2], out y);
					   	float.TryParse(Parts[3], out z);
					   	Caster.BlockPosition = new Vector3(x,y,z);
					}
					return true;
				}
				
				if(Parts[0] == "kill")
				{
				    if (Parts.Length == 1) LocalPlayer.Instance.Health = 0f;
					if(Parts[1] == "obj") World.QuestManager.Quest.NextObjective();
					return true;
				}

			    if (Parts[0] == "logItems")
			    {
			        for (var i = 0; i < LocalPlayer.Instance.Inventory.Length; i++)
			        {
                        if(LocalPlayer.Instance.Inventory[i] != null)
			                Log.WriteLine($" {i} {LocalPlayer.Instance.Inventory[i]}");
			        }
			        return true;
			    }

                if (Parts[0] == "xp"){
					LocalPlayer.Instance.XP += float.Parse(Parts[1]);
					return true;
				}
			    if (Parts[0] == "lvl")
			    {
			        LocalPlayer.Instance.Level = int.Parse(Parts[1]);
			        return true;
			    }

                if (Parts[0] == "speed"){
					LocalPlayer.Instance.Speed += float.Parse(Parts[1]);
					return true;
				}
			    if (Parts[0] == "attackspeed")
			    {
			        LocalPlayer.Instance.AttackSpeed = float.Parse(Parts[1]);
			        return true;
			    }
                if (Parts[0] == "dmg"){
					LocalPlayer.Instance.AttackPower += float.Parse(Parts[1]);
					return true;
				}
                if (Parts[0] == "drop")
                {
                    if (Parts[1] == "coin")
                    {
                        World.DropItem(ItemPool.Grab(ItemType.Gold), LocalPlayer.Instance.Position + LocalPlayer.Instance.Orientation * 16f);
                        return true;
                    }
                }
				
				if(Parts[0] == "sit"){
					LocalPlayer.Instance.Model.Sit();
					return true;
				}
			    if (Parts[0] == "spawnAnimation")
			    {
			        LocalPlayer.Instance.PlaySpawningAnimation = true;
			        return true;
			    }
				if(Parts[0] == "get"){
				    if (Parts[1] == "attackspeed")
				    {
				        Result = LocalPlayer.Instance.AttackSpeed.ToString(CultureInfo.InvariantCulture);
				        return true;
				    }
				    if (Parts[1] == "item")
				    {
				        LocalPlayer.Instance.Inventory.AddItem(ItemPool.Grab(Parts[2]));
                    }
                    if (Parts[1] == "sword")
				    {
				        LocalPlayer.Instance.Inventory.AddItem(ItemPool.Grab(new ItemPoolSettings(ItemTier.Divine, EquipmentType.Sword)));
				    }
				    if (Parts[1] == "axe")
				    {
				        LocalPlayer.Instance.Inventory.AddItem(ItemPool.Grab(new ItemPoolSettings(ItemTier.Divine, EquipmentType.Axe)));
				    }
				    if (Parts[1] == "katar")
				    {
				        LocalPlayer.Instance.Inventory.AddItem(ItemPool.Grab(new ItemPoolSettings(ItemTier.Divine, EquipmentType.Katar)));
				    }
				    if (Parts[1] == "hammer")
				    {
				        LocalPlayer.Instance.Inventory.AddItem(ItemPool.Grab(new ItemPoolSettings(ItemTier.Divine, EquipmentType.Hammer)));
				    }
				    if (Parts[1] == "claw")
				    {
				        LocalPlayer.Instance.Inventory.AddItem(ItemPool.Grab(new ItemPoolSettings(ItemTier.Divine, EquipmentType.Claw)));
				    }
				    if (Parts[1] == "blades")
				    {
				        LocalPlayer.Instance.Inventory.AddItem(ItemPool.Grab(new ItemPoolSettings(ItemTier.Divine, EquipmentType.DoubleBlades)));
				    }
				    if (Parts[1] == "bow")
				    {
				        LocalPlayer.Instance.Inventory.AddItem(ItemPool.Grab(new ItemPoolSettings(ItemTier.Divine, EquipmentType.Bow)));
				    }
				    if (Parts[1] == "knife")
				    {
				        LocalPlayer.Instance.Inventory.AddItem(ItemPool.Grab(new ItemPoolSettings(ItemTier.Divine, EquipmentType.Knife)));
				    }
				    if (Parts[1] == "ring")
				    {
				        LocalPlayer.Instance.Inventory.AddItem(ItemPool.Grab(new ItemPoolSettings(ItemTier.Divine, EquipmentType.Ring)));
				    }
                    if (Parts[1] == "gold")
				    {
				        var item = ItemPool.Grab(ItemType.Gold);
                        item.SetAttribute(CommonAttributes.Amount, int.Parse(Parts[2]));
                        LocalPlayer.Instance.Inventory.AddItem(item);
                    }
				    Result = $"Giving item {Parts[1].ToUpperInvariant()} to {Caster.Name}";
                    return true;
				}
				if(Parts[0] == "time"){
					if(Parts[1] == "speed"){
						Enviroment.SkyManager.DaytimeSpeed = int.Parse(Parts[2]);
						return true;
					}
					Enviroment.SkyManager.SetTime(int.Parse(Parts[1]));
					return true;
				}
			    if (Parts[0] == "burn")
			    {
			        LocalPlayer.Instance.AddComponent(new BurningComponent(LocalPlayer.Instance, null, 5f, 30f));
			    }
			    if (Parts[0] == "knock")
			    {
			        LocalPlayer.Instance.KnockForSeconds(float.Parse(Parts[1]));
			    }
                if (Parts[0] == "audiotest")
			    {
			        for (int i = 0; i < 16; i++)
			        {
			            World.SpawnMob(MobType.Boar, Caster.Position + Caster.Orientation * 16f, Utils.Rng);
			        }
			        return true;
			    }
			    if (Parts[0] == "audioareas")
			    {
			        for (int i = 0; i < SoundManager.SoundItems.Length; i++)
			            Log.WriteLine(SoundManager.SoundItems[i].Locked);
			        return true;
			    }
			    if (Parts[0] == "spawn"){
					if(Parts[1] == "bandit"){
						World.QuestManager.SpawnBandit(Caster.Position + Caster.Orientation * 32, false);
						return true;
					}
					if(Parts[1] == "plantling"){
						World.QuestManager.SpawnHumanoid(HumanType.Mandragora, Caster.Position + Caster.Orientation * 32);
						return true;
					}
					if(Parts[1] == "merchant"){
						World.QuestManager.SpawnHumanoid(HumanType.TravellingMerchant, Caster.Position + Caster.Orientation * 32);
						return true;
					}
			        if (Parts[1] == "ent")
			        {
			            World.QuestManager.SpawnEnt(Caster.Position + Caster.Orientation * 32);
			            return true;
			        }
                    if (Parts[1] == "carriage")
			        {
			            World.QuestManager.SpawnCarriage(Caster.Position + Caster.Orientation * 32);

                        return true;
			        }

			        if (World.MobFactory.ContainsFactory(Parts[1]))
			            World.SpawnMob(Parts[1], Caster.Position + Caster.Orientation * 32, Utils.Rng);
			        else
			            World.QuestManager.SpawnHumanoid(Parts[1], Caster.Position + Caster.Orientation * 32);
					return true;
				}
				if(Parts[0] == "chest"){
                    World.QuestManager.SpawnChest(Caster.Position + Caster.Orientation * 32, ItemPool.Grab(Parts[1]) );
					return true;
				}
			    Result = "Unknown command.";
                Log.WriteLine("Unknown command.");
			}catch(Exception e){
				Log.WriteLine(e.ToString());
			    Result = "Unknown command.";
                return false;
			}
			return false;
		}
	}
}
