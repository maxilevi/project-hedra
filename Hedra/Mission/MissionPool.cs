using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hedra.Engine.Scripting;
using System.Numerics;

namespace Hedra.Mission
{
    public static class MissionPool
    {
        private const string IsQuestName = "IS_QUEST";

        private static readonly List<IMissionDesign> MissionScripts;

        static MissionPool()
        {
            MissionScripts = new List<IMissionDesign>();
        }

        public static void Load()
        {
            var scripts = Interpreter.GetScripts("/Missions/");
            var missions = new List<IMissionDesign>();
            for (var i = 0; i < scripts.Length; ++i)
            {
                if(IsMission(scripts[i]))
                    missions.Add(new MissionDesign(scripts[i]));
            }
            Load(missions.ToArray());
        }

        public static void Load(IMissionDesign[] Designs)
        {
            MissionScripts.Clear();
            MissionScripts.AddRange(Designs);
        }

        public static IMissionDesign Grab(string Name)
        {
            return MissionScripts.First(M => M.Name == Name);
        }

        public static IMissionDesign Grab(Quests Name)
        {
            return Grab(Name.ToString());
        }

        public static IMissionDesign Random(Vector3 Position, QuestTier Tier = QuestTier.Any)
        {
            var possibilities = MissionScripts.Where(M => M.CanGive(Position)).ToArray();
            var map = new Dictionary<QuestTier, List<IMissionDesign>>();
            for (var i = 0; i < possibilities.Length; ++i)
            {
                var tier = possibilities[i].Tier;
                if(!map.ContainsKey(tier))
                    map[tier] = new List<IMissionDesign>();
                map[tier].Add(possibilities[i]);
            }
            if(possibilities.Length == 0)
                throw new ArgumentOutOfRangeException($"Failed to find quests that meet the given criteria");
            
            while (!map.ContainsKey(Tier) && Tier != QuestTier.Any) Tier--;
            return Tier == QuestTier.Any 
                ? possibilities[Utils.Rng.Next(0, possibilities.Length)] 
                : map[Tier][Utils.Rng.Next(0, map[Tier].Count)];
        }

        public static bool Exists(string Name)
        {
            return MissionScripts.Any(M => M.Name == Name);
        }
        
        private static bool IsMission(Script Mission)
        {
            return Mission.HasMember(IsQuestName);
        }

        public static IMissionDesign[] Designs => MissionScripts.ToArray();
    }

    public enum Quests
    {
        VisitSpawnVillage
    }

    public enum QuestTier
    {
        Any,
        Easy,
        Medium,
        Hard
    }
}