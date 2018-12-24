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
using Hedra.AISystem.Humanoid;
using Hedra.Core;
using OpenTK;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Player;
using Hedra.Engine.Generation;
using Hedra.Engine.Game;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.ModuleSystem.Templates;

namespace Hedra.Engine.WorldBuilding
{
    /// <summary>
    /// Description of QuestGenerator.
    /// </summary>
    public class WorldBuilding : IWorldBuilding
    {
        private readonly List<IGroundwork> _groundwork;
        private readonly List<BasePlateau> _plateaus;
        private readonly object _plateauLock = new object();
        private readonly object _groundworkLock = new object();

        public WorldBuilding()
        {
            _groundwork = new List<IGroundwork>();
            _plateaus = new List<BasePlateau>();
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
            human.Rotation = new Vector3(0, Utils.Rng.NextFloat(), 0) * 360f * Mathf.Radian;
            ApplySeasonHats(human, Type);
            return human;
        }

        public Humanoid SpawnVillager(Vector3 DesiredPosition, Random Rng)
        {
            var types = new []
            {
                HumanType.Warrior,
                HumanType.Rogue,
                HumanType.Mage,
                HumanType.Archer
            };
            var villager = World.WorldBuilding.SpawnHumanoid(types[Rng.Next(0, types.Length)], DesiredPosition);
            villager.SetWeapon(null);
            villager.SearchComponent<HealthBarComponent>().FontColor = HumanoidBehaviourTemplate.Friendly.ToColor();
            villager.Name = NameGenerator.PickMaleName(Rng);
            villager.SearchComponent<DamageComponent>().Immune = true;
            villager.SearchComponent<HealthBarComponent>().Name = villager.Name;
            return villager;
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
            var className = isGnoll
                ? HumanType.Gnoll.ToString()
                : Undead
                    ? HumanType.Skeleton.ToString()
                    : classType.ToString();
            var human = this.SpawnHumanoid(className, Level, Position, behaviour);
            if (isGnoll) human.AddonHealth = human.MaxHealth * .5f;

            if(!human.MainWeapon.Weapon.IsMelee)
                human.AddComponent( new ArcherAIComponent(human, Friendly) );
            else
                human.AddComponent(new WarriorAIComponent(human, Friendly));

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
            if (!string.Equals(Type, HumanType.Warrior.ToString(), StringComparison.InvariantCultureIgnoreCase) &&
                !string.Equals(Type, HumanType.Merchant.ToString(), StringComparison.InvariantCultureIgnoreCase) &&
                !string.Equals(Type, HumanType.TravellingMerchant.ToString(), StringComparison.InvariantCultureIgnoreCase) &&
                !string.Equals(Type, HumanType.Archer.ToString(), StringComparison.InvariantCultureIgnoreCase) &&
                !string.Equals(Type, HumanType.Blacksmith.ToString(), StringComparison.InvariantCultureIgnoreCase) &&
                !string.Equals(Type, HumanType.Rogue.ToString(), StringComparison.InvariantCultureIgnoreCase) &&
                !string.Equals(Type, HumanType.Mage.ToString(), StringComparison.InvariantCultureIgnoreCase)) return;
            
            if(Season.IsChristmas) 
                Human.SetHelmet(ItemPool.Grab(ItemType.ChristmasHat).Helmet);
        }

        private bool CanAddPlateau(RoundedPlateau Mount, RoundedPlateau[] Candidates)
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

        public bool CanAddPlateau(RoundedPlateau Mount)
        {
            lock (_plateauLock)
                return CanAddPlateau(Mount, _plateaus.Select(P => P as RoundedPlateau).Where(P => P != null).ToArray());
        }

        private void RemovePlateau(BasePlateau Mount)
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

        private void AddPlateau(BasePlateau Mount)
        {
            lock (_plateauLock)
            {
                Mount.MaxHeight = ApplyMultiple(Mount.Position, Mount.MaxHeight);
                _plateaus.Add(Mount);
            }         
        }

        public float ApplyMultiple(Vector2 Position, float MaxHeight, params BasePlateau[] Against)
        {
            var plateaus = Against.OrderByDescending(P => P.MaxHeight).ToList();
            for (var i = 0; i < plateaus.Count; i++)
            {
                MaxHeight = plateaus[i].Apply(Position, MaxHeight, out _);
            }
            return MaxHeight;
        }
        
        public float ApplyMultiple(Vector2 Position, float MaxHeight)
        {
            lock (_plateauLock)
                return ApplyMultiple(Position, MaxHeight, _plateaus.ToArray());
        }

        private void AddGroundwork(IGroundwork Work)
        {
            lock (_groundworkLock)
            {
                _groundwork.Add(Work);
            }
        }

        public BasePlateau[] GetPlateausFor(Vector2 Position)
        {
            lock (_plateauLock)
            {
                var chunkSpace = World.ToChunkSpace(Position);
                var list = new List<BasePlateau>();
                for (var i = 0; i < _plateaus.Count; ++i)
                {
                    var squared = _plateaus[i].ToBoundingBox();
                    if (
                        squared.Collides(chunkSpace)
                        || squared.Collides(chunkSpace + new Vector2(Chunk.Width, 0)) 
                        || squared.Collides(chunkSpace + new Vector2(0, Chunk.Width))
                        || squared.Collides(chunkSpace + new Vector2(Chunk.Width, Chunk.Width))
                        || World.ToChunkSpace(squared.BackCorner) == chunkSpace
                        || World.ToChunkSpace(squared.FrontCorner) == chunkSpace
                        || World.ToChunkSpace(squared.RightCorner) == chunkSpace
                        || World.ToChunkSpace(squared.LeftCorner) == chunkSpace
                    )
                    {
                        list.Add(_plateaus[i]);
                    }
                }
                return list.ToArray();
            }
        }

        public IGroundwork[] GetGroundworksFor(Vector2 Position)
        {
            lock (_groundworkLock)
            {
                var chunkSpace = World.ToChunkSpace(Position);
                var list = new List<IGroundwork>();
                for (var i = 0; i < _groundwork.Count; ++i)
                {
                    var squared = _groundwork[i].ToBoundingBox();
                    if (
                        squared.Collides(chunkSpace)
                        || squared.Collides(chunkSpace + new Vector2(Chunk.Width, 0)) 
                        || squared.Collides(chunkSpace + new Vector2(0, Chunk.Width))
                        || squared.Collides(chunkSpace + new Vector2(Chunk.Width, Chunk.Width))
                        || World.ToChunkSpace(squared.BackCorner) == chunkSpace
                        || World.ToChunkSpace(squared.FrontCorner) == chunkSpace
                        || World.ToChunkSpace(squared.RightCorner) == chunkSpace
                        || World.ToChunkSpace(squared.LeftCorner) == chunkSpace
                    )
                    {
                        list.Add(_groundwork[i]);
                    }
                }
                return list.ToArray();
            }
        }
        
        public BasePlateau[] Plateaux
        {
            get
            {    
                lock (_plateauLock)
                {
                    return _plateaus.ToArray();
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

        private static void LoopStructure(CollidableStructure Structure, Action<BasePlateau> PlateauDo, Action<IGroundwork> GroundworkDo)
        {
            if (Structure.Mountain != null)
                PlateauDo(Structure.Mountain);
            
            var plateaus = Structure.Plateaux.OrderByDescending(P => P.MaxHeight).ToArray();
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
