using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hedra.Engine.Scripting;
using OpenTK;

namespace Hedra.Mission
{
    public static class MissionPool
    {
        private const string IsQuestName = "IS_QUEST";

        private static readonly List<MissionDesign> MissionScripts;

        static MissionPool()
        {
            MissionScripts = new List<MissionDesign>();
        }

        public static void Load()
        {
            MissionScripts.Clear();
            var scripts = Interpreter.GetScripts("/Missions/");
            for (var i = 0; i < scripts.Length; ++i)
            {
                if(IsMission(scripts[i]))
                    MissionScripts.Add(new MissionDesign(scripts[i]));
            }
        }

        public static MissionDesign Grab(string Name)
        {
            return MissionScripts.First(M => M.Name == Name);
        }

        public static MissionDesign Grab(Quests Name)
        {
            return Grab(Name.ToString());
        }

        public static MissionDesign Random(Vector3 Position, QuestTier Tier = QuestTier.Any)
        {
            var possibilities = MissionScripts.Where(M => M.CanGive(Position) && (Tier == QuestTier.Any || M.Tier <= Tier)).ToArray();
            if(possibilities.Length == 0)
                throw new ArgumentOutOfRangeException($"Failed to find quests that meet the given criteria");
            return possibilities[Utils.Rng.Next(0, possibilities.Length)];
        }

        public static bool Exists(string Name)
        {
            return MissionScripts.Any(M => M.Name == Name);
        }
        
        private static bool IsMission(Script Mission)
        {
            return Mission.HasMember(IsQuestName);
        }

        public static MissionDesign[] Designs => MissionScripts.ToArray();
    }

    public enum Quests
    {
        VisitSpawnVillage
    }

    public enum QuestTier
    {
        Any,
        Easy,
        Medium
    }
}