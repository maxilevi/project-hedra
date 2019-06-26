using System;
using System.Drawing;
using System.Media;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.Localization;
using Hedra.Sound;
using SoundPlayer = Hedra.Sound.SoundPlayer;

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

        protected override QuestParameters BuildParameters(QuestObject Previous, QuestParameters Parameters, Random Rng)
        {
            Parameters.Set("Reward", Previous.Reward);
            return base.BuildParameters(Previous, Parameters, Rng);
        }

        private static DialogObject BuildDialog(QuestObject Quest)
        {
            var reward = Quest.Parameters.Get<QuestReward>("Reward");
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
            if (reward.HasSkillPoint)
                return new DialogObject
                {
                    Key = "quest_complete_reward_dialog",
                    Params = new object[]
                    {
                        $"{reward.SkillPoint} {Translations.Get("quest_skill_point")}"
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
            var reward = Quest.Parameters.Get<QuestReward>("Reward");
            if (reward.HasExperience)
            {
                Quest.Owner.XP += reward.Experience;
                Quest.Owner.ShowText($"+{reward.Experience} XP", Color.Violet, 20);
                SoundPlayer.PlayUISound(SoundType.NotificationSound);
            }
            if (reward.HasGold)
            {
                Quest.Owner.Gold += reward.Gold;
                SoundPlayer.PlayUISound(SoundType.TransactionSound);
            }

            if (reward.HasSkillPoint)
            {
                Quest.Owner.AbilityTree.ExtraSkillPoints += reward.SkillPoint;
                Quest.Owner.ShowText($"+{reward.SkillPoint} SP", Color.OrangeRed, 20);
                SoundPlayer.PlayUISound(SoundType.NotificationSound);
            }
            if (reward.HasItem)
            {
                Quest.Owner.AddOrDropItem(reward.Item);
                SoundPlayer.PlayUISound(SoundType.ItemEquip);
            }
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