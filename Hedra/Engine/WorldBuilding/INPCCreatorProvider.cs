using System;
using System.Numerics;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Mission;

namespace Hedra.Engine.WorldBuilding
{
    public interface INPCCreatorProvider
    {
        Humanoid SpawnHumanoid(HumanType Type, Vector3 DesiredPosition);
        Humanoid SpawnHumanoid(string Type, Vector3 DesiredPosition);
        Humanoid SpawnVillager(Vector3 DesiredPosition, Random Rng);
        Humanoid SpawnVillager(Vector3 DesiredPosition, int Seed);
        Humanoid SpawnVillager(HumanType Type, Vector3 DesiredPosition, int Seed);
        IHumanoid SpawnQuestGiver(HumanType Type, Vector3 Position, IMissionDesign Quest, Random Rng);
        IHumanoid SpawnQuestGiver(Vector3 Position, IMissionDesign Quest, Random Rng);
        Humanoid SpawnBandit(Vector3 Position, int Level, BanditOptions Options);
    }
}