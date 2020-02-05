using Hedra.Engine.Player;
using Hedra.Engine.Scripting;
using Hedra.EntitySystem;
using Hedra.Mission.Blocks;
using System.Numerics;

namespace Hedra.Mission
{
    public class MissionDesign : IMissionDesign
    {
        private readonly Script _design;
        
        public MissionDesign(Script Design)
        {
            _design = Design;
        }

        public MissionObject Build(IHumanoid Giver, IPlayer Owner)
        {
            var builder = _design.Execute<MissionBuilder>("setup_timeline", Settings.Position, Giver, Owner, Utils.Rng);
            if (builder == null) return null;
            if (builder.ReturnToComplete)
            {
                builder.Next(new EndMission(builder.Reward)
                {
                    Humanoid = Giver,
                    Giver = Giver,
                    Owner = Owner
                });
            }
            builder.SetSettings(new MissionSettings
            {
                IsStoryline = IsStoryline,
                CanSave = CanSave,
                Name = Name
            });
            return builder.Mission;
        }

        public bool CanGive(Vector3 Position)
        {
            return _design.Execute<bool>("can_give", Position);
        }
        
        public QuestPriority Priority => _design.HasMember("QUEST_PRIORITY") ? _design.Get<QuestPriority>("QUEST_PRIORITY") : QuestPriority.Normal; 
        public bool CanSave => _design.HasMember("CAN_SAVE") && _design.Get<bool>("CAN_SAVE");
        public bool IsStoryline => _design.HasMember("IS_STORYLINE") && _design.Get<bool>("IS_STORYLINE");
        public QuestHint Hint =>
            _design.HasMember("QUEST_HINT") ? _design.Get<QuestHint>("QUEST_HINT") : QuestHint.NoHint;
        public QuestTier Tier => _design.Get<QuestTier>("QUEST_TIER");
        public string Name => _design.Get<string>("QUEST_NAME");
        public MissionDesignSettings Settings { get; set; }
        public IMissionDesign Clone => new MissionDesign(_design)
        {
            Settings = Settings
        };
    }
}