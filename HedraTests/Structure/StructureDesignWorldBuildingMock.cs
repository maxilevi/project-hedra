using System;
using Hedra;
using Hedra.Components;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Items;
using System.Numerics;

namespace HedraTests.Structure
{
    public class StructureDesignWorldBuildingMock : IWorldBuilding
    {
        public bool CanAddPlateau(RoundedPlateau Mount)
        {
            return false;
        }

        public BasePlateau[] GetPlateausFor(Vector2 Position)
        {
            throw new System.NotImplementedException();
        }

        public IGroundwork[] GetGroundworksFor(Vector2 Position)
        {
            throw new System.NotImplementedException();
        }

        public BasePlateau[] Plateaux => null;
        
        public IGroundwork[] Groundworks => null;

        public Humanoid SpawnVillager(Vector3 DesiredPosition, Random Rng)
        {
            return SpawnHumanoid(HumanType.Warrior.ToString(), DesiredPosition, null);
        }

        public Humanoid SpawnVillager(Vector3 DesiredPosition, int Seed)
        {
            return SpawnVillager(DesiredPosition, new Random(Seed));
        }

        public Humanoid SpawnHumanoid(HumanType Type, Vector3 DesiredPosition)
        {
            return SpawnHumanoid(Type.ToString(), DesiredPosition);
        }

        public Humanoid SpawnHumanoid(string Type, Vector3 DesiredPosition)
        {
            return SpawnHumanoid(Type, DesiredPosition, null);
        }

        public Humanoid SpawnHumanoid(string Type, Vector3 DesiredPosition, HumanoidConfiguration Configuration)
        {
            var human = new Humanoid();
            human.Position = DesiredPosition;
            human.AddComponent(new HealthBarComponent(human, string.Empty, HealthBarType.Neutral));
            World.AddEntity(human);
            return human;
        }

        public Humanoid SpawnBandit(Vector3 Position, int Level, bool Friendly = false, bool Undead = false)
        {
            return SpawnHumanoid(null, Position);
        }

        public Humanoid SpawnVillager(Vector3 Position, bool Move, string Name)
        {
            return SpawnHumanoid(null, Position);
        }

        public Chest SpawnChest(Vector3 Position, Item Item)
        {
            return new Chest(Position, Item);
        }

        public string GenerateName()
        {
            return string.Empty;
        }

        public void SetupStructure(CollidableStructure Structure)
        {    
        }

        public void DisposeStructure(CollidableStructure Structure)
        {
        }

        public float ApplyMultiple(Vector2 Position, float MaxHeight)
        {
            return MaxHeight;
        }

        public float ApplyMultiple(Vector2 Position, float MaxHeight, params BasePlateau[] Against)
        {
            return MaxHeight;
        }
    }
}