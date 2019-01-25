using System;
using Hedra.Components;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.EntitySystem;

namespace Hedra.Engine.QuestSystem
{
    public class QuestGiverComponent : QuestComponent
    {
        private readonly QuestObject _quest;
        private readonly TalkComponent _talk;
        private readonly QuestThoughtsComponent _thoughts;
        private bool _canGiveQuest = true;
        
        public QuestGiverComponent(IHumanoid Parent, QuestObject Quest) : base(Parent)
        {
            if(Parent.SearchComponent<TalkComponent>() != null)
                throw new ArgumentException("There can only be 1 talk component");
            _quest = Quest;
            Parent.ShowIcon(CacheItem.AttentionIcon);
            Parent.AddComponent(_thoughts = _quest.BuildThoughts(Parent));
            Parent.AddComponent(_talk = new TalkComponent(Parent));
            _talk.OnTalkingStarted += T =>
            {
                Quest.SetupDialog();
            };
            _talk.OnTalkingEnded += AddQuest;
        }

        public override void Update()
        {
            _talk.CanTalk = _canGiveQuest;
        }

        private void OnQuestCompleted(QuestObject Object)
        {
            if(Object == _quest)
                Parent.RemoveComponent(this);
        }
        
        private void OnQuestAbandoned(QuestObject Object)
        {
            if (Object == _quest)
                Parent.RemoveComponent(this);
        }

        private void AddQuest(IEntity Interactee)
        {
            if(!(Interactee is IPlayer player) || (Parent.Position - player.Position).LengthSquared > 32 * 32) return;
            _canGiveQuest = false;
            player.Questing.Start(_quest);
            player.Questing.QuestAbandoned += OnQuestAbandoned;
            player.Questing.QuestCompleted += OnQuestCompleted;
        }

        public override void Dispose()
        {
            if (_quest.Owner != null)
            {
                _quest.Owner.Questing.QuestAbandoned -= OnQuestAbandoned;
                _quest.Owner.Questing.QuestCompleted -= OnQuestCompleted;
            }
            Parent.RemoveComponent(_talk);
            Parent.RemoveComponent(_thoughts);
            Parent.ShowIcon(null);
            base.Dispose();
        }
    }
}