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
        Humanoid SpawnBandit(Vector3 Position, int Level, bool Friendly = false, bool Undead = false);
        Humanoid SpawnVillager(Vector3 Position, bool Move, string Name = null);
        Humanoid SpawnEnt(Vector3 Position);
        Chest SpawnChest(Vector3 Position, Item Item);
        string GenerateName();
        void SetupStructure(CollidableStructure Structure);
        void DisposeStructure(CollidableStructure Structure);
    }

}