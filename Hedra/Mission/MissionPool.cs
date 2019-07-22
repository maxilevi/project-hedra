using System.Collections.Generic;
using System.IO;
using Hedra.Engine.Scripting;

namespace Hedra.Mission
{
    public static class MissionPool
    {
        private const string SetupTimelineName = "setup_timeline";
        private const string CanGiveName = "can_give";
        private const string IsQuestName = "IS_QUEST";

        private static readonly List<Script> MissionScripts;

        static MissionPool()
        {
            MissionScripts = new List<Script>();
        }

        public static void Load()
        {
            MissionScripts.Clear();
            var scripts = Interpreter.GetScripts("/Missions/");
            for (var i = 0; i < scripts.Length; ++i)
            {
                if(IsMission(scripts[i]))
                    MissionScripts.Add(scripts[i]);
            }
        }
        
        
        private static bool IsMission(Script Mission)
        {
            return Mission.HasMember(IsQuestName);
        }
    }
}