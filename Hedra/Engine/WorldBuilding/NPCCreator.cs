using System;
using System.Numerics;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Mission;
using Hedra.Rendering;

namespace Hedra.Engine.WorldBuilding
{
    public static class NPCCreator
    {
        public static Vector4[] HairColors { get; private set; }

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

        static NPCCreator()
        {
            const int columnCount = 8;
            const int rowCount = 8;
            var allColors = new Vector4[columnCount * rowCount];
            const float step = 360f / (columnCount-1);
            for (var j = 0; j < rowCount; ++j)
            {
                for (var i = 1; i < columnCount; ++i)
                {
                    allColors[j * columnCount + i] =
                        Colors.HsLtoRgba((i-1) * step, 1.0f, 1.0f - (j + 1) / (float)(rowCount + 2), 1f);
                }
                // Generate all the grays
                allColors[j * columnCount] = Colors.HsLtoRgba(0, 0, 1.0f - (j + 1) / (float)(rowCount + 2), 1f);
            }

            HairColors = allColors;
        }
    }
}