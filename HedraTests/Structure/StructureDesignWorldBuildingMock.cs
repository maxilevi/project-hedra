using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace HedraTests.Structure
{
    public class StructureDesignWorldBuildingMock : IWorldBuilding
    {
        public bool CanAddPlateau(Plateau Mount)
        {
            return false;
        }

        public void AddPlateau(Plateau Mount)
        {
        }

        public void AddGroundwork(IGroundwork Work)
        {
        }

        public Plateau[] Plateaus => null;
        
        public IGroundwork[] Groundworks => null;
        
        public Entity SpawnCarriage(Vector3 Position)
        {
            return null;
        }

        public Humanoid SpawnHumanoid(HumanType Type, Vector3 DesiredPosition)
        {
            return SpawnHumanoid(Type.ToString(), DesiredPosition);
        }

        public Humanoid SpawnHumanoid(string Type, Vector3 DesiredPosition)
        {
            return SpawnHumanoid(Type, DesiredPosition, null);
        }

        public Humanoid SpawnHumanoid(string Type, Vector3 DesiredPosition, HumanoidBehaviourTemplate behaviour)
        {
            var human = new Humanoid();
            human.Position = DesiredPosition;
            human.AddComponent(new HealthBarComponent(human, string.Empty));
            human.AddComponent(new DamageComponent(human));
            World.AddEntity(human);
            return human;
        }

        public Humanoid SpawnBandit(Vector3 Position, bool Friendly, bool Undead)
        {
            return SpawnHumanoid(null, Position);
        }

        public Humanoid SpawnBandit(Vector3 Position, bool Friendly)
        {
            return SpawnBandit(Position, Friendly, false);
        }

        public Humanoid SpawnVillager(Vector3 Position, bool Move)
        {
            return SpawnBandit(Position, Move);
        }

        public Humanoid SpawnVillager(Vector3 Position, bool Move, string Name)
        {
            return SpawnHumanoid(null, Position);
        }

        public Humanoid SpawnEnt(Vector3 Position)
        {
            return SpawnHumanoid(null, Position);
        }

        public Chest SpawnChest(Vector3 Position, Item Item)
        {
            var chest = new Chest(Position, Item);
            World.AddStructure(chest);
            return chest;
        }

        public string GenerateName()
        {
            return string.Empty;
        }
    }
}