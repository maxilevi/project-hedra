using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;

namespace Hedra.Engine.QuestSystem.Designs.Auxiliaries
{
    public class EndDesign : SpeakDesign
    {
        protected override bool HasNext => false;

        protected override QuestObject Setup(QuestObject Object)
        {
            return base.Setup(Object);
        }

        protected override QuestSpeakComponent BuildSpeakComponent(QuestObject Quest)
        {
            var dialog = BuildDialog(Quest);
            return new QuestSpeakComponent(
                Quest.Giver,
                dialog.Key,
                dialog.Params
            );
        }

        private static DialogObject BuildDialog(QuestObject Quest)
        {
            var reward = Quest.Reward;
            if (reward.HasExperience)
                return new DialogObject
                {
                    Key = "quest_complete_reward_dialog",
                    Params = new object[]
                    {
                        $"{reward.Experience} {Translations.Get("quest_experience")}"
                    }
                };
            if (reward.HasGold)
                return new DialogObject
                {
                    Key = "quest_complete_reward_dialog",
                    Params = new object[]
                    {
                        $"{reward.Gold} {Translations.Get("quest_gold")}"
                    }
                };
            if (reward.HasItem)
                return new DialogObject
                {
                    Key = "quest_complete_reward_dialog",
                    Params = new object[]
                    {
                        MakeItemString(reward.Item)
                    }
                };
            return new DialogObject
            {
                Key = "quest_complete_dialog",
                Params = new object[0]
            };
        }

        private static string MakeItemString(Item Item)
        {
            var amount = Item.HasAttribute(CommonAttributes.Amount) 
                ? Item.GetAttribute<int>(CommonAttributes.Amount) 
                : 0; 
            return amount > 0 
                ? $"{amount} {Item.DisplayName.ToUpperInvariant()}"
                : Item.DisplayName.ToUpperInvariant();
        }
        
        private static void GiveReward(QuestObject Quest)
        {
            var reward = Quest.Reward;
            if (reward.HasExperience)
                Quest.Owner.XP += reward.Experience;
            if (reward.HasGold)
                Quest.Owner.Gold += reward.Gold;
            if (reward.HasItem)
                Quest.Owner.AddOrDropItem(reward.Item);
        }
        
        protected override void Consume(QuestObject Quest)
        {
            base.Consume(Quest);
            GiveReward(Quest);
            Quest.Giver.ShowIcon(null);
        }
        
        private struct DialogObject
        {
            public string Key { get; set; }
            public object[] Params { get; set; }
        }
    }
}