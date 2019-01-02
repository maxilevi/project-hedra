using System;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Player.QuestSystem;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.Engine.Rendering;
using Hedra.Rendering;

namespace Hedra.Engine.QuestSystem.Designs
{
    public class CraftDesign : QuestDesign
    {
        public override QuestTier Tier => QuestTier.Easy;
        public override string Name => "CraftingQuest";

        public override string ThoughtsKeyword => "quest_craft_dialog";

        public override object[] GetThoughtsParameters(QuestObject Quest)
        {
            return new object[]
            {
            };
        }
        
        public override string GetShortDescription(QuestObject Quest)
        {
            throw new NotImplementedException();
        }

        public override string GetDescription(QuestObject Quest)
        {
            throw new NotImplementedException();
        }

        public override QuestView BuildView(QuestObject Quest)
        {
            throw new NotImplementedException();
        }

        protected override QuestParameters BuildParameters(QuestContext Context, QuestParameters Parameters, Random Rng)
        {
            throw new NotImplementedException();
        }

        protected override QuestDesign[] Auxiliaries { get; }
        protected override QuestDesign[] Descendants { get; }

        public override bool IsQuestCompleted(QuestObject Object)
        {
            throw new NotImplementedException();
        }

        protected override void Consume(QuestObject Object)
        {
            throw new NotImplementedException();
        }
    }
}