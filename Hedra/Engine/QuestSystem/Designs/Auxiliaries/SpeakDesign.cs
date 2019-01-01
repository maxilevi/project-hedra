using System;
using Hedra.Components;
using Hedra.Engine.Localization;
using Hedra.Engine.Player.QuestSystem;
using Hedra.EntitySystem;
using Hedra.Rendering;

namespace Hedra.Engine.QuestSystem.Designs.Auxiliaries
{
    public class SpeakDesign : AuxiliaryDesign
    {
        public override string Name => "TalkQuest";
        
        public override string GetShortDescription(QuestObject Quest)
        {
            return Translations.Get(
                "quest_speak_short",
                Quest.Giver?.Name ?? "NULL"
            );
        }

        public override string GetDescription(QuestObject Quest)
        {
            return Translations.Get(
                "quest_speak_description",
                Quest.Giver?.Name ?? "NULL"
            );
        }
        
        protected override QuestParameters BuildParameters(QuestContext Context, QuestParameters Parameters, Random Rng)
        {
            return Parameters;
        }

        public override bool IsQuestCompleted(QuestObject Object)
        {
            return Object.Parameters.Get<IHumanoid>("Humanoid").SearchComponent<QuestSpeakComponent>().Spoke;
        }

        protected override void Consume(QuestObject Object)
        {
            Object.Parameters.Get<IHumanoid>("Humanoid").RemoveComponent(
                Object.Parameters.Get<IHumanoid>("Humanoid").SearchComponent<QuestSpeakComponent>()
            );
        }

        public override VertexData BuildPreview(QuestObject Object)
        {
            return VertexData.Empty;
        }
    }
}