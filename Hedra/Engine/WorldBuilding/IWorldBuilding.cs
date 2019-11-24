using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Items;
using System.Numerics;
using Hedra.API;

namespace Hedra.Engine.WorldBuilding
{
    public interface IWorldBuilding
    {
        BasePlateau[] GetPlateausFor(Vector2 Position);
        IGroundwork[] GetGroundworksFor(Vector2 Position);
        BasePlateau[] Plateaux { get; }
        IGroundwork[] Groundworks { get; }
        Humanoid SpawnVillager(Vector3 DesiredPosition, Random Rng);
        Humanoid SpawnVillager(Vector3 DesiredPosition, int Seed);
        Humanoid SpawnHumanoid(HumanType Type, Vector3 DesiredPosition);
        Humanoid SpawnHumanoid(string Type, Vector3 DesiredPosition);
        Humanoid SpawnBandit(Vector3 Position, int Level, BanditOptions Options);
        Chest SpawnChest(Vector3 Position, Item Item);
        string GenerateName();
        void SetupStructure(CollidableStructure Structure);
        void DisposeStructure(CollidableStructure Structure);
        float ApplyMultiple(Vector2 Position, float MaxHeight);
        float ApplyMultiple(Vector2 Position, float MaxHeight, params BasePlateau[] Against);
    }
}