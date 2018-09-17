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
using System.Linq;
using OpenTK;
using Hedra.Engine.AISystem;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Generation;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ModuleSystem;

namespace Hedra.Engine.WorldBuilding
{
	/// <summary>
	/// Description of QuestGenerator.
	/// </summary>
	public class WorldBuilding : IWorldBuilding
	{
	    private readonly List<IGroundwork> _groundwork;
	    private readonly List<Plateau> _plateaus;
        private readonly object _plateauLock = new object();
	    private readonly object _groundworkLock = new object();

	    public WorldBuilding()
	    {
	        _groundwork = new List<IGroundwork>();
            _plateaus = new List<Plateau>();
        }

	    public bool CanAddPlateau(Plateau Mount)
	    {
			lock (_plateauLock)
			{
				for (var i = 0; i < _plateaus.Count; i++)
				{
					if (_plateaus[i].Collides(Mount) && Math.Abs(_plateaus[i].MaxHeight - Mount.MaxHeight) > 2.0f)
					{
						return false;
					}
				}
				return true;
			}
	    }

	    public void AddPlateau(Plateau Mount)
	    {
	        try
	        {
	            lock (_plateauLock)
	            {
	                _plateaus.Add(Mount);
	            }
	        }
	        finally
	        {
	            this.SortPlateaus();
	        }
	    }

	    private void SortPlateaus()
	    {
	        // Plateaus should be clamped to the lowest one
	        /*lock (_plateauLock)
	        {
		        var doneSet = new HashSet<Plateau>();
		        for (var i = 0; i < _plateaus.Count; i++)
		        {
			        var intersecting = new List<Plateau>();
			        for (var j = 0; j < _plateaus.Count; j++)
			        {
                        if(_plateaus[j] == _plateaus[i]) continue;
						if(!doneSet.Contains(_plateaus[j]) && (_plateaus[j].Position - _plateaus[i].Position).LengthFast < _plateaus[i].Radius + _plateaus[j].Radius)
							intersecting.Add(_plateaus[j]);
			        }
			        var lowest = intersecting.OrderByDescending(P => P.Radius).ToArray();
			        for (var j = 0; j < intersecting.Count; j++)
			        {
				       // _plateaus[j].MaxHeight = lowest[0].MaxHeight;
				        doneSet.Add(_plateaus[j]);
			        }
		        }
	        }*/
	    }

        public void AddGroundwork(IGroundwork Work)
	    {
	        lock (_groundworkLock)
	        {
	            _groundwork.Add(Work);
	        }
	    }
		
		public Plateau[] Plateaus
		{
			get
			{
				lock (_plateauLock)
				{
					return _plateaus.Select(P => P).ToArray();
				}
			}
		}

	    public IGroundwork[] Groundworks
	    {
	        get
	        {
	            lock (_groundworkLock)
	                return _groundwork.ToArray();
	        }
	    }


        public Entity SpawnCarriage(Vector3 Position)
	    {
	        Entity carriage = World.SpawnMob("QuestCarriage", Position, 1);
	        carriage.Health = carriage.MaxHealth;
	        carriage.Physics.CanCollide = true;
	        carriage.Physics.PushAround = false;
	        carriage.RemoveComponent(carriage.SearchComponent<BasicAIComponent>());
	        carriage.SearchComponent<DamageComponent>().Immune = true;
	        carriage.AddComponent(new CarriageAIComponent(carriage));
            carriage.RemoveComponent(carriage.SearchComponent<HealthBarComponent>());

	        World.AddEntity(carriage);
	        return carriage;
	    }

	    public Humanoid SpawnHumanoid(HumanType Type, Vector3 DesiredPosition)
	    {
	        return SpawnHumanoid(Type.ToString(), DesiredPosition, null);
	    }

	    public Humanoid SpawnHumanoid(string Type, Vector3 DesiredPosition)
	    {
	        var human = HumanoidFactory.BuildHumanoid(Type, null);
	        human.Physics.TargetPosition = World.FindPlaceablePosition(human, DesiredPosition);
            return human;
	    }

        public Humanoid SpawnHumanoid(string Type, Vector3 DesiredPosition, HumanoidBehaviourTemplate behaviour)
	    {
	        var human = HumanoidFactory.BuildHumanoid(Type, behaviour);
	        human.Physics.TargetPosition = World.FindPlaceablePosition(human, DesiredPosition);
	        return human;
	    }

	    public Humanoid SpawnBandit(Vector3 Position, bool Friendly, bool Undead)
	    {
            int classN = Utils.Rng.Next(0, ClassDesign.AvailableClasses.Length);
	        var classType = ClassDesign.FromType(ClassDesign.AvailableClasses[classN]);

	        var behaviour = new HumanoidBehaviourTemplate(HumanoidBehaviourTemplate.Hostile)
	        {
	            Name = Undead ? "Skeleton" : "Bandit"
	        };
	        var isGnoll = Utils.Rng.Next(0, 4) == 1;
            var human = this.SpawnHumanoid(isGnoll ? "Gnoll" : classType.ToString(), Position, behaviour);
	        if (isGnoll) human.AddonHealth = human.MaxHealth * .5f;
	        if (Undead)
	        {
	            human.Model = new HumanoidModel(human, HumanType.Skeleton);
	            human.Model.SetWeapon(human.MainWeapon.Weapon);
	        }

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

        public Humanoid SpawnVillager(Vector3 Position, bool Move)
        {
	    	return this.SpawnVillager(Position, Move, null);
	    }
	    
	    public Humanoid SpawnVillager(Vector3 Position, bool Move, string Name)
	    {
	        var behaviour = new HumanoidBehaviourTemplate(HumanoidBehaviourTemplate.Hostile);
	        behaviour.Name = Name;
            var human = this.SpawnHumanoid(HumanType.Villager, Position);

	        human.AddComponent(new VillagerAIComponent(human, Move));
	        return human;
	    }

	    public Humanoid SpawnEnt(Vector3 Position)
	    {
	        var behaviour = new HumanoidBehaviourTemplate(HumanoidBehaviourTemplate.Hostile);
	        var human = this.SpawnHumanoid("Ent", Position, behaviour);
	        human.AddComponent(new WarriorAIComponent(human, false));
	        human.MainWeapon = null;

	        var region = World.BiomePool.GetRegion(Position);
	        var woodColor = region.Colors.WoodColors[Utils.Rng.Next(0, region.Colors.WoodColors.Length)] * 2.0f;
            human.Model.Paint(new []
            {
                woodColor,
                region.Colors.LeavesColors[Utils.Rng.Next(0, region.Colors.LeavesColors.Length)],
                woodColor * .65f
            });

	        return human;
        }
	    
	    public Chest SpawnChest(Vector3 Position, Item Item)
	    {
	    	var chest = new Chest(Position, Item);
	        World.AddStructure(chest);
	    	return chest;
	    }

		public string GenerateName()
        {
			var rng = new Random(World.Seed);
			var types = new string[]{"Islands","Lands","Mountains"};
			return types[rng.Next(0,types.Length)]+" of "+NameGenerator.Generate(World.Seed);
		}
	}
}
