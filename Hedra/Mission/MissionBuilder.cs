using System;
using System.Collections.Generic;
using Hedra.Engine.QuestSystem;
using Hedra.Mission.Blocks;

namespace Hedra.Mission
{
    public delegate void OnMissionEnd();

    public delegate void OnMissionStart();

    public delegate void OnMissionDispose();

    public class MissionBuilder
    {
        public Func<bool> FailWhen { get; set; }
        public OnMissionDispose MissionDispose;
        public event OnMissionStart MissionStart;
        public event OnMissionEnd MissionEnd;
        private readonly List<MissionBlock> _designs;
        private MissionObject _mission;

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

        public void SetSettings(MissionSettings New)
        {
            Settings = New;
        }

        public QuestReward Reward { get; private set; } = new QuestReward();
        
        public MissionSettings Settings { get; private set; } = new MissionSettings();

        public MissionObject Mission
        {
            get
            {
                if (_mission != null) return _mission;
                if(_designs.Count == 0)
                    throw new ArgumentOutOfRangeException($"A mission needs at least 1 mission block");
                var mission = new MissionObject(_designs.ToArray(), _designs[0].OpeningDialog, Settings);
                mission.MissionEnd += MissionEnd;
                mission.MissionDispose += MissionDispose;
                mission.MissionStart += MissionStart;
                mission.FailWhen = FailWhen;
                mission.QuestType = Settings.Name;
                return _mission = mission;
            }
        }

        public bool ReturnToComplete { get; set; } = true;
    }
}