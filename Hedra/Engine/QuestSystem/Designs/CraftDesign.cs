using System;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Player.QuestSystem;
using Hedra.Engine.Rendering;
using Hedra.Rendering;

namespace Hedra.Engine.QuestSystem.Designs
{
    public class CraftDesign : QuestDesign
    {
        public override QuestTier Tier => QuestTier.Easy;
        public override string Name => "CraftingQuest";

        public override string GetShortDescription(QuestObject Quest)
        {
            throw new NotImplementedException();
        }

        public override string GetDescription(QuestObject Quest)
        {
            throw new NotImplementedException();
        }

        public override QuestView View { get; }
        protected override QuestParameters BuildParameters(QuestContext Context, QuestParameters Parameters, Random Rng)
        {
            throw new NotImplementedException();
        }

        public override QuestDesign[] Predecessors { get; }
        public override QuestDesign[] Auxiliaries { get; }
        public override QuestDesign[] Descendants { get; }
        public override string ToString(QuestObject Object)
        {
            throw new NotImplementedException();
        }

        public override bool IsQuestCompleted(QuestObject Object)
        {
            throw new NotImplementedException();
        }

        protected override void Consume(QuestObject Object)
        {
            throw new NotImplementedException();
        }

        public override VertexData BuildPreview(QuestObject Object)
        {
            throw new NotImplementedException();
        }
    }
}