using System;
using System.Numerics;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Mission;

namespace Hedra.Engine.WorldBuilding
{
    public static class NPCCreator
    {
        public static INPCCreatorProvider Provider { get; set; } = new NPCCreatorProvider();

        public static Humanoid SpawnHumanoid(HumanType Type, Vector3 DesiredPosition)
        {
            return Provider.SpawnHumanoid(Type, DesiredPosition);
        }

        public static Humanoid SpawnHumanoid(string Type, Vector3 DesiredPosition)
        {
            return Provider.SpawnHumanoid(Type, DesiredPosition);
        }

        public static Humanoid SpawnVillager(Vector3 DesiredPosition, Random Rng)
        {
            return Provider.SpawnVillager(DesiredPosition, Rng);
        }

        public static Humanoid SpawnVillager(Vector3 DesiredPosition, int Seed)
        {
            return Provider.SpawnVillager(DesiredPosition, Seed);
        }

        public static Humanoid SpawnVillager(HumanType Type, Vector3 DesiredPosition, int Seed)
        {
            return Provider.SpawnVillager(Type, DesiredPosition, Seed);
        }

        public static IHumanoid SpawnQuestGiver(HumanType Type, Vector3 Position, IMissionDesign Quest, Random Rng)
        {
            return Provider.SpawnQuestGiver(Type, Position, Quest, Rng);
        }

        public static IHumanoid SpawnQuestGiver(Vector3 Position, IMissionDesign Quest, Random Rng)
        {
            return Provider.SpawnQuestGiver(Position, Quest, Rng);
        }

        public static Humanoid SpawnBandit(Vector3 Position, int Level, BanditOptions Options)
        {
            return Provider.SpawnBandit(Position, Level, Options);
        }
    }
}