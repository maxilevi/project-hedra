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
        bool CanAddPlateau(RoundedPlateau Mount);
        BasePlateau[] GetPlateausFor(Vector2 Position);
        IGroundwork[] GetGroundworksFor(Vector2 Position);
        BasePlateau[] Plateaux { get; }
        IGroundwork[] Groundworks { get; }
        Humanoid SpawnHumanoid(HumanType Type, Vector3 DesiredPosition);
        Humanoid SpawnHumanoid(string Type, Vector3 DesiredPosition);
        Humanoid SpawnBandit(Vector3 Position, int Level, bool Friendly = false, bool Undead = false);
        Chest SpawnChest(Vector3 Position, Item Item);
        string GenerateName();
        void SetupStructure(CollidableStructure Structure);
        void DisposeStructure(CollidableStructure Structure);
        float ApplyMultiple(Vector3 Position, float MaxHeight);
        float ApplyMultiple(Vector3 Position, float MaxHeight, params BasePlateau[] Against);
    }
}