using System.Drawing;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.QuestSystem;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.Localization;
using Hedra.Sound;

namespace Hedra.Mission.Blocks
{
    public class EndMission : TalkMission
    {
        private readonly QuestReward _reward;
        
        public EndMission(QuestReward Reward) : base(DialogFromReward(Reward))
        {
            _reward = Reward;
        }

        protected override void OnTalkingEnded(IEntity Talker)
        {
            base.OnTalkingEnded(Talker);
            Owner.Questing.Trigger();
        }

        private static DialogObject DialogFromReward(QuestReward Reward)
        {
            if (Reward.HasExperience)
                return new DialogObject
                {
                    Keyword = "quest_complete_reward_dialog",
                    Arguments = new object[]
                    {
                        $"{Reward.Experience} {Translations.Get("quest_experience")}"
                    }
                };
            if (Reward.HasGold)
                return new DialogObject
                {
                    Keyword = "quest_complete_reward_dialog",
                    Arguments = new object[]
                    {
                        $"{Reward.Gold} {Translations.Get("quest_gold")}"
                    }
                };
            if (Reward.HasItem)
                return new DialogObject
                {
                    Keyword = "quest_complete_reward_dialog",
                    Arguments = new object[]
                    {
                        MakeItemString(Reward.Item)
                    }
                };
            if (Reward.HasSkillPoint)
                return new DialogObject
                {
                    Keyword = "quest_complete_reward_dialog",
                    Arguments = new object[]
                    {
                        $"{Reward.SkillPoint} {Translations.Get("quest_skill_point")}"
                    }
                };
            return new DialogObject
            {
                Keyword = "quest_complete_dialog",
                Arguments = new object[0]
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
        
        private void GiveReward()
        {
            if (_reward.HasExperience)
            {
                Owner.XP += _reward.Experience;
                Owner.ShowText($"+{_reward.Experience} XP", Color.Violet, 20);
                SoundPlayer.PlayUISound(SoundType.NotificationSound);
            }
            if (_reward.HasGold)
            {
                Owner.Gold += _reward.Gold;
                SoundPlayer.PlayUISound(SoundType.TransactionSound);
            }

            if (_reward.HasSkillPoint)
            {
                Owner.AbilityTree.ExtraSkillPoints += _reward.SkillPoint;
                Owner.ShowText($"+{_reward.SkillPoint} SP", Color.OrangeRed, 20);
                SoundPlayer.PlayUISound(SoundType.NotificationSound);
            }
            if (_reward.HasItem)
            {
                Owner.AddOrDropItem(_reward.Item);
                SoundPlayer.PlayUISound(SoundType.ItemEquip);
            }
        }

        public override void Cleanup()
        {
            base.Cleanup();
            GiveReward();
            Giver.ShowIcon(null);
        }
    }
}