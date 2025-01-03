using System.Collections.Generic;
using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;

namespace Hedra.Engine.WorldBuilding
{
    public interface IWorldBuilding
    {
        BasePlateau[] Plateaux { get; }
        List<Landform> Landforms { get; }
        IGroundwork[] Groundworks { get; }
        BasePlateau[] GetPlateausFor(Vector2 Position);
        IEnumerable<Landform> GetLandformsFor(Vector2 Position);
        IGroundwork[] GetGroundworksFor(Vector2 Position);
        Chest SpawnChest(Vector3 Position, Item Item);
        string GenerateName();
        void SetupStructure(CollidableStructure Structure);
        void DisposeStructure(CollidableStructure Structure);
        float ApplyMultiple(Vector2 Position, float MaxHeight);
        float ApplyMultiple(Vector2 Position, float MaxHeight, params BasePlateau[] Against);
        void RemoveLandform(Landform Land);
        void AddLandform(Landform Land);
    }
}