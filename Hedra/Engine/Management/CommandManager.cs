/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 30/12/2016
 * Time: 05:46 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
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
		
		public static bool ProcessCommand(string Command, Entity Caster){
			try{
				//remove the /
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
				if(Parts[0] == "dmg"){
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
				    if (Parts[1] == "sword")
				    {
				        LocalPlayer.Instance.Inventory.AddItem(ItemPool.Grab(new ItemPoolSettings(ItemTier.Common, WeaponType.Sword)));
				    }
					/*if(Parts[1] == "mount"){
						if(Parts[2] == "horse")
							LocalPlayer.Instance.Inventory.AddItem(new InventoryItem(ItemType.Mount, new ItemInfo(Material.HorseMount, 100)));
						else if(Parts[2] == "wolf")
							LocalPlayer.Instance.Inventory.AddItem(new InventoryItem(ItemType.Mount, new ItemInfo(Material.WolfMount, 100)));
					}else if(Parts[1] == "axe")
						LocalPlayer.Instance.Inventory.AddItem(new InventoryItem(ItemType.Axe, ItemInfo.Random(ItemType.Axe)));
					else if(Parts[1] == "hammer")
						LocalPlayer.Instance.Inventory.AddItem(new InventoryItem(ItemType.Hammer, ItemInfo.Random(ItemType.Hammer)));
					else if(Parts[1] == "food")
						LocalPlayer.Instance.Inventory.AddItem(new InventoryItem(ItemType.Food, ItemInfo.Berry(1)));
					else if(Parts[1] == "knife")
						LocalPlayer.Instance.Inventory.AddItem(new InventoryItem(ItemType.Knife, ItemInfo.Random(ItemType.Knife)));
					else if(Parts[1] == "katar")
						LocalPlayer.Instance.Inventory.AddItem(new InventoryItem(ItemType.Katar, ItemInfo.Random(ItemType.Katar)));
					else if(Parts[1] == "claw")
						LocalPlayer.Instance.Inventory.AddItem(new InventoryItem(ItemType.Claw, ItemInfo.Random(ItemType.Claw)));
					else if(Parts[1] == "blade")
						LocalPlayer.Instance.Inventory.AddItem(new InventoryItem(ItemType.DoubleBlades, ItemInfo.Random(ItemType.DoubleBlades)));
					else if(Parts[1] == "glider")
						LocalPlayer.Instance.Inventory.AddItem(new InventoryItem(ItemType.Glider, ItemInfo.Random(ItemType.Glider)));
					else if(Parts[1] == "coin")
						LocalPlayer.Instance.Inventory.AddItem(new InventoryItem(ItemType.Coin, ItemInfo.Gold(5)) );
					else if (Parts[1] == "rapier")
					    LocalPlayer.Instance.Inventory.AddItem(new InventoryItem(ItemType.Sword, ItemInfo.Random(ItemType.Sword)));*/
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
                Log.WriteLine("Unknown command.");
			}catch(Exception e){
				Log.WriteLine(e.ToString());
				return false;
			}
			return false;
		}
	}
}
