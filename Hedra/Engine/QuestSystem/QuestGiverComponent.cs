using System;
using Hedra.Components;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Game;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Game;

namespace Hedra.Engine.QuestSystem
{
    public class QuestGiverComponent : QuestComponent
    {
        private readonly QuestObject _quest;
        private readonly TalkComponent _talk;
        private QuestThoughtsComponent _thoughts;
        private bool _canGiveQuest = true;
        
        public QuestGiverComponent(IHumanoid Parent, QuestObject Quest) : base(Parent)
        {
            if(Parent.SearchComponent<TalkComponent>() != null)
                throw new ArgumentException("There can only be 1 talk component");
            _quest = Quest;
            Parent.ShowIcon(CacheItem.AttentionIcon);
            Parent.AddComponent(_talk = new TalkComponent(Parent));
            _talk.OnTalkingEnded += AddQuest;
            _talk.OnTalkingStarted += T =>
            {
                if(!(T is IPlayer player)) return;
                _quest.GenerateContent(player);
                Parent.AddComponent(_thoughts = _quest.BuildThoughts(Parent));
                Quest.SetupDialog();
            };
        }

        public override void Update()
        {
            _talk.CanTalk = _canGiveQuest;
            if (Parent.IsNear(GameManager.Player, 16) && !_talk.Talking)
            {
                Parent.LookAt(GameManager.Player);
            }
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
                Parent.RemoveComponent(_thoughts);
            }
            Parent.RemoveComponent(_talk);
            Parent.ShowIcon(null);
            base.Dispose();
        }
    }
}