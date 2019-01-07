using System;
using Hedra.Components;
using Hedra.Engine.Localization;
using Hedra.Engine.Player.QuestSystem;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.EntitySystem;
using Hedra.Rendering;

namespace Hedra.Engine.QuestSystem.Designs.Auxiliaries
{
    public class SpeakDesign : AuxiliaryDesign
    {
        
        
        public override string GetShortDescription(QuestObject Quest)
        {
            return Translations.Get(
                "quest_speak_short",
                Quest.Giver.Name
            );
        }

        public override string GetDescription(QuestObject Quest)
        {
            return Translations.Get(
                "quest_speak_description",
                Quest.Giver.Name
            );
        }

        protected virtual QuestSpeakComponent BuildSpeakComponent(QuestObject Quest)
        {
            return new QuestSpeakComponent(
                Quest.Giver,
                Quest.Parameters.Get<QuestObject>("NextObject"),
                Quest.Parameters.Get<QuestDesign>("Next")
            );
        }

        protected override QuestObject Setup(QuestObject Quest)
        {
            var component = BuildSpeakComponent(Quest);
            component.Spoke += T =>
            {
                Quest.Parameters.Set("IsCompleted", true);
                Quest.Owner.Questing.Trigger();
            };
            component.BeforeSpeaking += T =>
            {
                var nextQuest = Quest.Parameters.Get<QuestObject>("NextObject");
                var nextDesign = Quest.Parameters.Get<QuestDesign>("Next");
                nextDesign?.SetupDialog(nextQuest, Quest.Owner);
            };
            Quest.Giver.AddComponent(component);
            Quest.Parameters.Set("IsCompleted", false);
            return Quest;
        }

        public override bool IsQuestCompleted(QuestObject Object)
        {
            return Object.Parameters.Get<bool>("IsCompleted");
        }

        public override void Abandon(QuestObject Quest)
        {
            Consume(Quest);
        }
        
        protected override void Consume(QuestObject Quest)
        {
            Quest.Giver.RemoveComponent(
                Quest.Giver.SearchComponent<QuestSpeakComponent>()
            );
        }

        public override QuestView BuildView(QuestObject Object)
        {
            return new EntityView(Object, Object.Giver.Model);
        }
    }
}