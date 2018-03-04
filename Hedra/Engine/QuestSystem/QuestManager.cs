/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/07/2016
 * Time: 12:02 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Rendering;
using OpenTK;
using System.Drawing;
using Hedra.Engine.Player;
using Hedra.Engine.Generation;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Item;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.QuestSystem.Objectives;
using Hedra.Engine.Scenes;

namespace Hedra.Engine.QuestSystem
{
	/// <summary>
	/// Description of QuestGenerator.
	/// </summary>
	public class QuestManager : IUpdatable
	{
		
		private bool _addedToMenu;
		private bool _goalSent = false;
		public bool QuestCompleted {get; set;}
		public int ChainLength {get; private set;}
		public const int MaxChainLength = 9;
		public Random Generator {get; private set;}

		public Vector3 ObjectivePosition;
		public Objective Quest;
	    public Dictionary<Vector3, float> VillagePositions = new Dictionary<Vector3, float>();
	    public List<Plateau> Plateaus = new List<Plateau>();
	    public List<Objective> PassedObjectives = new List<Objective>();
		public EntityMesh QuestIcon;
		
		public QuestManager(){
			Quest = new DummyObjective();
			
			UpdateManager.Add(this);
		}

	    public void AddPlateau(Plateau Mount)
	    {
	        lock(Plateaus)
                Plateaus.Add(Mount);
	    }

	    public void AddVillagePosition(Vector3 Position, float Radius)
	    {
	        lock(VillagePositions)
	            VillagePositions.Add( Position, Radius);

        }

	    public Entity SpawnCarriage(Vector3 Position)
	    {
	        Entity carriage = World.SpawnMob("QuestCarriage", Position, 1);
	        carriage.Health = carriage.MaxHealth;
	        carriage.Physics.CanCollide = true;
	        carriage.Physics.PushAround = false;
	        carriage.RemoveComponent(carriage.SearchComponent<AIComponent>());
	        carriage.SearchComponent<DamageComponent>().Immune = true;
	        carriage.AddComponent(new CarriageAIComponent(carriage));
            carriage.RemoveComponent(carriage.SearchComponent<HealthBarComponent>());

	        World.AddEntity(carriage);
	        return carriage;
	    }

	    public Humanoid SpawnHumanoid(HumanType Type, Vector3 Position)
	    {
	        return SpawnHumanoid(Type.ToString(), Position, null);
	    }

	    public Humanoid SpawnHumanoid(string Type, Vector3 Position)
	    {
	        var human = HumanoidFactory.BuildHumanoid(Type, null);
	        return human;
	    }

        public Humanoid SpawnHumanoid(string Type, Vector3 Position, HumanoidBehaviourTemplate behaviour)
	    {
	        var human = HumanoidFactory.BuildHumanoid(Type, behaviour);
	        human.Physics.TargetPosition = Position;
	        return human;
	    }

	    public Humanoid SpawnBandit(Vector3 Position, bool Friendly, bool Undead)
	    {
            int classN = Utils.Rng.Next(0, 3);
	        Class classType = classN == 0 ? Class.Archer : classN == 1 ? Class.Warrior : Class.Rogue;

	        var behaviour = new HumanoidBehaviourTemplate(HumanoidBehaviourTemplate.Hostile);
	        behaviour.Name = Undead ? "Skeleton" : "Bandit";
            var human = this.SpawnHumanoid("Gnoll"/*classType.ToString()*/, Position, behaviour);

	        if (Undead)
	        {
	            human.Model = new HumanModel(human, HumanType.Skeleton);
	            human.Model.SetWeapon(human.MainWeapon.Weapon);
	        }
	        human.Level = Math.Max(1, LocalPlayer.Instance.Level - 1);

            if(!human.MainWeapon.Weapon.IsMelee)
                human.AddComponent( new ArcherAIComponent(human, Friendly) );
            else
                human.AddComponent(new WarriorAIComponent(human, Friendly));

	        return human;
	    }


	    public Humanoid SpawnBandit(Vector3 Position, bool Friendly)
	    {
	        return this.SpawnBandit(Position, Friendly, false);
	    }

        public Humanoid SpawnVillager(Vector3 Position, bool Move){
	    	return this.SpawnVillager(Position, Move, null);
	    }
	    
	    public Humanoid SpawnVillager(Vector3 Position, bool Move, string Name)
	    {
	        var behaviour = new HumanoidBehaviourTemplate(HumanoidBehaviourTemplate.Hostile);
	        behaviour.Name = Name;
            var human = this.SpawnHumanoid(HumanType.Villager, Position);

	        human.AddComponent(new VillagerAIComponent(human, Move));

	        World.AddEntity(human);
	        return human;
	    }
	    
	    public Chest SpawnChest(Vector3 Position, InventoryItem Item){
	    	var chest = new Chest(Position, Item);
	        World.AddStructure(chest);
	    	return chest;
	    }
	    
	    public bool WasQuestTypeDone(Type QuestType){
	    	for(int i = 0; i < PassedObjectives.Count; i++){
	    		if( PassedObjectives[i].GetType() == QuestType)
	    			return true;
	    	}
	    	return false;
	    }
	    
	    public void EndRun(){
	    	Sound.SoundManager.PlaySoundInPlayersLocation(Sound.SoundType.NotificationSound);
			LocalPlayer.Instance.MessageDispatcher.ShowTitleMessage("QUEST COMPLETED", 3f, Color.Gold);
			float Exp = ChainLength * 7;
			this.QuestCompleted = true;
			SceneManager.Game.LPlayer.CanInteract = false;
			TaskManager.Delay(2500, delegate{ 
		        var label = new Billboard(4.0f, "+"+Exp+" XP", Color.Violet, FontCache.Get(AssetManager.Fonts.Families[0], 48, FontStyle.Bold),
				SceneManager.Game.LPlayer.Position);
				label.Size = .4f;
				label.Vanish = true;
			});
			
			
	    }
		
	    public void Recreate(){
			//Quest.Recreate();
		}
		
		public void Update(){

			if(Quest.IsLost || this.QuestCompleted)
			{
			    LocalPlayer.Instance.MessageDispatcher.ShowMessageWhile(Networking.NetworkManager.IsConnected ? "[R] Disconnect" : "[R] New Run",
			        () => Quest.IsLost || this.QuestCompleted);
			}
				
		}
		
		public void SetQuest(Objective Quest){
			//this.Quest = Quest;
			//Quest.Recreate();
		}
		
		public Objective GenerateQuest()
		{
		    return new DummyObjective();
			//Build Quest Chain
			this.QuestCompleted = false;
			Objective quest = null;
			Generator = new Random(World.Seed+232);
			ChainLength = Generator.Next(4, MaxChainLength);
			int n = Generator.Next(0,6);

			switch(n){
				case 0:
					quest = new RecoverItemObjective(TempleType.RandomTemple);
					break;
				case 1:
					quest = new BossObjective();
					break;
				case 2:
					quest = new RescueHumanObjective();
					break;
				case 3:
				    quest = new VillageObjective();
					break;
				case 4:
				    quest = new CollectItemsObjective();
					break;
			    case 5:
			        quest = new ClearCementeryObjective();
			        break;
              /*  case 6:
                    quest = new RecoverBlacksmithHammerObjective();
                    break;
			    case 7:
			        quest = new RecoverCarriageObjective();
			        break;*/

            }
			return quest;
		}
	    
	    public InventoryItem RandomPrize(Random Rng){
    		var rewardType = ItemType.Ring;
			int n = Rng.Next(0, 17);
			if(n < 4)
				rewardType = ItemType.Bow;
			else if (n < 8)
				rewardType = ItemType.Sword;
			else if (n < 12)
				rewardType = ItemType.Katar;
			else if ( n == 12 ){
				rewardType = ItemType.Mount;
			}else if ( n > 12 && n < 17){
				rewardType = ItemType.Hammer;
			}
			
			InventoryItem item;
			if(rewardType == ItemType.Mount)
				item = new InventoryItem( rewardType, (Rng.NextBool() ) ? new ItemInfo(Material.WolfMount, 1) : new ItemInfo(Material.HorseMount, 1) );
			else
				item = new InventoryItem( rewardType, ItemInfo.Random(rewardType, Rng) ); 
    		return item;

	    }
		
		public string GenerateName(){
			var rng = new Random(World.Seed);
			var types = new string[]{"Islands","Lands","Mountains"};
			return types[rng.Next(0,types.Length)]+" of "+NameGenerator.Generate(World.Seed);
		}
	}
}
