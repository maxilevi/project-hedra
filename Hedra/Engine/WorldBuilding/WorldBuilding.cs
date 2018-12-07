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
using Hedra.AISystem;
using Hedra.Components;
using OpenTK;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Generation;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Game;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ModuleSystem;
using Hedra.EntitySystem;

namespace Hedra.Engine.WorldBuilding
{
    /// <summary>
    /// Description of QuestGenerator.
    /// </summary>
    public class WorldBuilding : IWorldBuilding
    {
        private readonly List<IGroundwork> _groundwork;
        private List<Plateau> _plateaus;
        private readonly object _plateauLock = new object();
        private readonly object _groundworkLock = new object();

        public WorldBuilding()
        {
            _groundwork = new List<IGroundwork>();
            _plateaus = new List<Plateau>();
        }

        public Entity SpawnCarriage(Vector3 Position)
        {
            var carriage = World.SpawnMob("QuestCarriage", Position, 1);
            carriage.Health = carriage.MaxHealth;
            carriage.Physics.CollidesWithStructures = true;
            carriage.Physics.PushAround = false;
            carriage.RemoveComponent(carriage.SearchComponent<BasicAIComponent>());
            carriage.SearchComponent<DamageComponent>().Immune = true;
            carriage.AddComponent(new CarriageAIComponent(carriage));
            carriage.RemoveComponent(carriage.SearchComponent<HealthBarComponent>());
            carriage.Removable = false;
            World.AddEntity(carriage);
            return carriage;
        }

        public Humanoid SpawnHumanoid(HumanType Type, Vector3 DesiredPosition)
        {
            return SpawnHumanoid(Type.ToString(), 1, DesiredPosition, null);
        }

        public Humanoid SpawnHumanoid(string Type, Vector3 DesiredPosition)
        {
            return SpawnHumanoid(Type, 1, DesiredPosition, null);
        }

        private Humanoid SpawnHumanoid(string Type, int Level, Vector3 DesiredPosition, HumanoidBehaviourTemplate Behaviour)
        {
            var human = HumanoidFactory.BuildHumanoid(Type, Level, Behaviour);
            human.Physics.TargetPosition = World.FindPlaceablePosition(human, DesiredPosition);
            ApplySeasonHats(human, Type);
            return human;
        }

        public Humanoid SpawnBandit(Vector3 Position, int Level, bool Friendly = false, bool Undead = false)
        {
            int classN = Utils.Rng.Next(0, ClassDesign.AvailableClasses.Length);
            var classType = ClassDesign.FromType(ClassDesign.AvailableClasses[classN]);

            var behaviour = new HumanoidBehaviourTemplate(HumanoidBehaviourTemplate.Hostile)
            {
                Name = Undead ? "Skeleton" : "Bandit"
            };
            var isGnoll = Utils.Rng.Next(0, 4) == 1;
            var human = this.SpawnHumanoid(isGnoll ? "Gnoll" : classType.ToString(), Level, Position, behaviour);
            if (isGnoll) human.AddonHealth = human.MaxHealth * .5f;
            if (Undead)
            {
                human.Model = new HumanoidModel(human, HumanType.Skeleton);
                human.SetWeapon(human.MainWeapon.Weapon);
            }

            if(!human.MainWeapon.Weapon.IsMelee)
                human.AddComponent( new ArcherAIComponent(human, Friendly) );
            else
                human.AddComponent(new WarriorAIComponent(human, Friendly));

            return human;
        }
        
        public Humanoid SpawnVillager(Vector3 Position, bool Move, string Name)
        {
            var behaviour = new HumanoidBehaviourTemplate(HumanoidBehaviourTemplate.Hostile);
            behaviour.Name = Name;
            var human = this.SpawnHumanoid(HumanType.Villager, Position);

            human.AddComponent(new VillagerAIComponent(human, Move));
            return human;
        }
        
        public Chest SpawnChest(Vector3 Position, Item Item)
        {
            return new Chest(Position, Item);
        }

        public string GenerateName()
        {
            var rng = new Random(World.Seed);
            var types = new []
            {
                "Lands","Mountains", "Territory"
            };
            return $"{types[rng.Next(0,types.Length)]} of {NameGenerator.Generate(World.Seed)}";
        }

        private void ApplySeasonHats(Humanoid Human, string Type)
        {
            if (Type.ToLowerInvariant() != HumanType.Warrior.ToString().ToLowerInvariant() &&
                Type.ToLowerInvariant() != HumanType.Merchant.ToString().ToLowerInvariant() &&
                Type.ToLowerInvariant() != HumanType.TravellingMerchant.ToString().ToLowerInvariant() &&
                Type.ToLowerInvariant() != HumanType.Archer.ToString().ToLowerInvariant() &&
                Type.ToLowerInvariant() != HumanType.Blacksmith.ToString().ToLowerInvariant() &&
                Type.ToLowerInvariant() != HumanType.Rogue.ToString().ToLowerInvariant() &&
                Type.ToLowerInvariant() != HumanType.Mage.ToString().ToLowerInvariant() &&
                Type.ToLowerInvariant() != HumanType.Villager.ToString().ToLowerInvariant()) return;
            
            if(Season.IsChristmas) 
                Human.SetHelmet(ItemPool.Grab(CommonItems.ChristmasHat).Helmet);
        }

        public bool CanAddPlateau(Plateau Mount, Plateau[] Candidates)
        {

            for (var i = 0; i < Candidates.Length; i++)
            {
                if (Candidates[i].Collides(Mount) && Math.Abs(Candidates[i].MaxHeight - Mount.MaxHeight) > 2.0f)
                {
                    return false;
                }
            }

            return true;
        }

        public bool CanAddPlateau(Plateau Mount)
        {
            lock (_plateauLock)
                return CanAddPlateau(Mount, _plateaus.ToArray());
        }

        private void RemovePlateau(Plateau Mount)
        {
            lock (_plateauLock)
            {
                _plateaus.Remove(Mount);
            }
        }

        private void RemoveGroundwork(IGroundwork Work)
        {
            lock (_groundworkLock)
            {
                _groundwork.Remove(Work);
            }
        }

        private void AddPlateau(Plateau Mount)
        {
            try
            {
                lock (_plateauLock)
                {
                    ApplyMultiple(Mount);
                    _plateaus.Add(Mount);
                }
            }
            finally
            {
                //lock (_plateauLock)
                //    _plateaus = _plateaus.OrderByDescending(P => P.MaxHeight).ToList();
            }
        }

        private void ApplyMultiple(Plateau Mount)
        {
            var plateaus = _plateaus.OrderByDescending(P => P.MaxHeight).ToList();
            for (var i = 0; i < plateaus.Count; i++)
            {
                Mount.MaxHeight = plateaus[i].Apply(Mount.Position.Xz, Mount.MaxHeight, out _);
            }
        }

        private void AddGroundwork(IGroundwork Work)
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

        private void LoopStructure(CollidableStructure Structure, Action<Plateau> PlateauDo, Action<IGroundwork> GroundworkDo)
        {
            if (Structure.Mountain != null)
                PlateauDo(Structure.Mountain);
            
            var plateaus = Structure.Plateaus.OrderByDescending(P => P.MaxHeight).ToArray();
            for (var i = 0; i < plateaus.Length; i++)
                PlateauDo(plateaus[i]);
            
            var groundworks = Structure.Groundworks;
            for (var i = 0; i < groundworks.Length; i++)
                GroundworkDo(groundworks[i]);
        }
        
        public void SetupStructure(CollidableStructure Structure)
        {
            LoopStructure(Structure, AddPlateau, AddGroundwork);
        }

        public void DisposeStructure(CollidableStructure Structure)
        {
            LoopStructure(Structure, RemovePlateau, RemoveGroundwork);
        }
    }
}
