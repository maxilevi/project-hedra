using Hedra.Engine.Player;
using Hedra.Engine.Scripting;
using Hedra.EntitySystem;
using Hedra.Mission.Blocks;
using OpenTK;

namespace Hedra.Mission
{
    public class MissionDesign : IMissionDesign
    {
        private readonly Script _design;
        
        public MissionDesign(Script Design)
        {
            _design = Design;
        }

        public MissionObject Build(Vector3 Position, IHumanoid Giver, IPlayer Owner)
        {
            var builder = _design.Execute<MissionBuilder>("setup_timeline", Position, Giver, Owner, Utils.Rng);
            if (builder.ReturnToComplete)
            {
                builder.Next(new EndMission(builder.Reward)
                {
                    Humanoid = Giver,
                    Giver = Giver,
                    Owner = Owner
                });
            }
            return builder.Mission;
        }

        public bool CanGive(Vector3 Position)
        {
            return _design.Execute<bool>("can_give", Position);
        }
        
        public QuestTier Tier => _design.Get<QuestTier>("QUEST_TIER");
        public string Name => _design.Get<string>("QUEST_NAME");
    }
}