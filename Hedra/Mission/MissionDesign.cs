using System;
using Hedra.Engine.Player;
using Hedra.Engine.Scripting;
using Hedra.EntitySystem;
using Hedra.Mission.Blocks;
using OpenTK;

namespace Hedra.Mission
{
    public class MissionDesign
    {
        private readonly Script _design;
        
        public MissionDesign(Script Design)
        {
            _design = Design;
        }

        public MissionObject Build(Vector3 Position, IHumanoid Giver, IPlayer Owner)
        {
            var builder = _design.Execute<MissionBuilder>("setup_timeline", Position, Giver, Owner, Utils.Rng);
            builder.Next(new EndMission()
            {
                Humanoid = Giver,
                Giver = Giver,
                Owner = Owner
            });
            return builder.Mission;
        }

        public string Name => _design.Get<string>("QUEST_NAME");
    }
}