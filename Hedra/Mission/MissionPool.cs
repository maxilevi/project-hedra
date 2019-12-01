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

        public static IMissionDesign Random(Vector3 Position, QuestTier Tier = QuestTier.Any, QuestHint Hint = QuestHint.InvalidHint)
        {
            var possibilities = MissionScripts.Where(M => M.CanGive(Position) && !M.IsStoryline).ToArray();
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
                ? SelectWeightedRandom(possibilities, Hint, Utils.Rng)
                : SelectWeightedRandom(map[Tier], Hint, Utils.Rng);
        }

        private static IMissionDesign SelectWeightedRandom(IList<IMissionDesign> Possibilities, QuestHint Hint, Random Rng)
        {
            var entries = new List<IMissionDesign>();
            for (var i = 0; i < Possibilities.Count; ++i)
            {
                entries.Add(Possibilities[i]);
                /* Add a double entry for those who match the hint */
                if(Hint == Possibilities[i].Hint)
                    entries.Add(Possibilities[i]);
            }

            return entries[Rng.Next(0, entries.Count)];
        }

        public static bool Exists(string Name)
        {
            return MissionScripts.Any(M => M.Name == Name);
        }
        
        private static bool IsMission(Script Mission)
        {
            return Mission.HasMember(IsQuestName) && Mission.Get<bool>(IsQuestName);
        }

        public static IMissionDesign[] Designs => MissionScripts.ToArray();
    }

    public enum Quests
    {
        VisitSpawnVillage,
        TheBeginning
    }

    public enum QuestTier
    {
        Any,
        Easy,
        Medium,
        Hard
    }

    public enum QuestHint
    {
        NoHint,
        Fishing,
        Magic,
        InvalidHint
    }
}