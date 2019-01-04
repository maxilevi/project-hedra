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

        protected override QuestObject Setup(QuestObject Object)
        {
            var component = new QuestSpeakComponent(
                Object.Giver,
                Object.Parameters.Get<QuestObject>("NextObject"),
                Object.Parameters.Get<QuestDesign>("Next")
            );
            component.Spoke += T =>
            {
                Object.Parameters.Set("IsCompleted", true);
                Object.Owner.Questing.Trigger();
            };
            Object.Giver.AddComponent(component);
            Object.Parameters.Set("IsCompleted", false);
            return Object;
        }

        public override bool IsQuestCompleted(QuestObject Object)
        {
            return Object.Parameters.Get<bool>("IsCompleted");
        }

        public override void Abandon(QuestObject Object)
        {
            Consume(Object);
        }
        
        protected override void Consume(QuestObject Object)
        {
            Object.Giver.RemoveComponent(
                Object.Giver.SearchComponent<QuestSpeakComponent>()
            );
        }

        public override QuestView BuildView(QuestObject Object)
        {
            return new EntityView(Object, Object.Giver.Model);
        }
    }
}