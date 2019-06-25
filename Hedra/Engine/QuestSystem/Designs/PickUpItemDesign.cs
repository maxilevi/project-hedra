using System;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.Engine.QuestSystem.Designs.Auxiliaries;
using Hedra.Localization;

namespace Hedra.Engine.QuestSystem.Designs
{
    public class PickUpItemDesign : PassthroughDesign
    {
        public override bool CanSaveOnThisStep => false;
        public const string PickupParameterName = "DeliveryItem";
        public override QuestTier Tier => QuestTier.Easy;
        public override string Name => "PickUpItemQuest";
        
        public override string GetShortDescription(QuestObject Quest)
        {
            var itemCollect = Quest.Parameters.Get<ItemCollect>(PickupParameterName);
            return Translations.Get("quest_pickup_item_short", ItemPool.Grab(itemCollect.Name).DisplayName);
        }

        public override string GetDescription(QuestObject Quest)
        {
            var itemCollect = Quest.Parameters.Get<ItemDescription>(PickupParameterName);
            if (itemCollect.PickupMessage != null) return itemCollect.PickupMessage;
            return Translations.Get(
                "quest_pickup_default_item_description",
                $"{itemCollect.Amount} {ItemPool.Grab(itemCollect.Name).DisplayName}"
            );
        }

        public override QuestView BuildView(QuestObject Quest)
        {
            var itemCollect = Quest.Parameters.Get<ItemCollect>(PickupParameterName);
            var item = ItemPool.Grab(itemCollect.Name);
            return new ModelView(Quest, item.Model);
        }

        protected override QuestParameters BuildParameters(QuestObject Previous, QuestParameters Parameters, Random Rng)
        {
            Parameters.Set(PickupParameterName, Previous.Parameters.Get<ItemCollect>(PickupParameterName));
            return base.BuildParameters(Previous, Parameters, Rng);
        }

        protected override QuestDesign[] GetAuxiliaries(QuestObject Quest) => new QuestDesign[]
        {
            new SpeakDesign()
        };

        public override bool IsQuestCompleted(QuestObject Quest)
        {
            return Quest.Parameters.Get<ItemCollect>(PickupParameterName).IsCompleted(Quest.Owner, out _);
        }
    }
}