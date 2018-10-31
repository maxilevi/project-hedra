using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.Player;
using OpenTK;

namespace Hedra.Engine.WorldBuilding
{
    public interface IWorldBuilding
    {
        bool CanAddPlateau(Plateau Mount);
        bool CanAddPlateau(Plateau Mount, Plateau[] Candidates);
        Plateau[] Plateaus { get; }
        IGroundwork[] Groundworks { get; }
        Entity SpawnCarriage(Vector3 Position);
        Humanoid SpawnHumanoid(HumanType Type, Vector3 DesiredPosition);
        Humanoid SpawnHumanoid(string Type, Vector3 DesiredPosition);
        Humanoid SpawnHumanoid(string Type, Vector3 DesiredPosition, HumanoidBehaviourTemplate Behaviour);
        Humanoid SpawnBandit(Vector3 Position, bool Friendly, bool Undead);
        Humanoid SpawnBandit(Vector3 Position, bool Friendly);
        Humanoid SpawnVillager(Vector3 Position, bool Move);
        Humanoid SpawnVillager(Vector3 Position, bool Move, string Name);
        Humanoid SpawnEnt(Vector3 Position);
        Chest SpawnChest(Vector3 Position, Item Item);
        string GenerateName();
        void SetupStructure(CollidableStructure Structure);
        void DisposeStructure(CollidableStructure Structure);
    }

}