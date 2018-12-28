using System;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Player.QuestSystem;

namespace Hedra.Engine.QuestSystem.Designs
{
    public class CraftDesign : QuestDesign
    {
        public override QuestTier Tier => QuestTier.Easy;
        public override Translation OpeningLine { get; }

        public override QuestView View { get; }
        protected override QuestParameters BuildParameters(QuestContext Context, QuestParameters Parameters, Random Rng)
        {
            throw new NotImplementedException();
        }

        public override QuestDesign[] Predecessors { get; }
        public override QuestDesign[] Descendants { get; }
        public override string ToString(QuestObject Object)
        {
            throw new NotImplementedException();
        }

        public override bool IsQuestCompleted(QuestObject Object, IPlayer Player)
        {
            throw new NotImplementedException();
        }
    }
}