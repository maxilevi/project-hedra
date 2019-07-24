using System.Collections.Generic;
using Hedra.Engine.QuestSystem;
using Hedra.Mission.Blocks;

namespace Hedra.Mission
{
    public delegate void OnMissionEnd();

    public class MissionBuilder
    {
        public event OnMissionEnd MissionEnd;
        private readonly List<MissionBlock> _designs;
        private string _thoughtsKeyword;
        private object[] _thoughtsArguments;

        public MissionBuilder()
        {
            _designs = new List<MissionBlock>();
        }

        public void Next(MissionBlock Design)
        {
            _designs.Add(Design);
        }

        public void SetReward(QuestReward New)
        {
            Reward = New;
        }

        public void SetOpeningDialog(string Keyword, object[] Arguments)
        {
            _thoughtsKeyword = Keyword;
            _thoughtsArguments = Arguments;
        }

        public QuestReward Reward { get; private set; }

        public MissionObject Mission
        {
            get
            {
                var mission = new MissionObject(_designs.ToArray(), _thoughtsKeyword, _thoughtsArguments);
                mission.MissionEnd += MissionEnd;
                return mission;
            }
        }
    }
}