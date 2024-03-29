using System;
using Hedra.Components;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Mission;

namespace Hedra.Engine.QuestSystem
{
    public class QuestGiverComponent : QuestComponent
    {
        private readonly IMissionDesign _questArchetype;
        private readonly TalkComponent _talk;
        private bool _canGiveQuest = true;
        private MissionObject _quest;

        public QuestGiverComponent(IHumanoid Parent, IMissionDesign QuestArchetype) : base(Parent)
        {
            if (Parent.SearchComponent<TalkComponent>() != null)
                throw new ArgumentException("There can only be 1 talk component");
            _questArchetype = QuestArchetype;
            Parent.ShowIcon(AlertIcon);
            _talk = new TalkComponent(Parent);
            _talk.OnTalkingStarted += CreateQuest;
            _talk.OnTalkingEnded += AddQuest;
            Parent.AddComponent(_talk);
        }

        protected virtual CacheItem AlertIcon => CacheItem.AttentionIcon;

        public override void Update()
        {
            _talk.CanTalk = _canGiveQuest;
            if (Parent.IsNear(GameManager.Player, 16) && !_talk.Talking && !Parent.IsMoving)
                Parent.LookAt(GameManager.Player);
        }

        private void OnQuestCompleted(MissionObject Object)
        {
            if (Object == _quest)
                Parent.RemoveComponent(this);
        }

        private void OnQuestAbandoned(MissionObject Object)
        {
            if (Object == _quest)
                Parent.RemoveComponent(this);
        }

        private void AddQuest(IEntity Talker)
        {
            if (!(Talker is IPlayer player) || (Parent.Position - player.Position).LengthSquared() > 32 * 32 ||
                _quest == null) return;
            _canGiveQuest = false;
            player.Questing.Start(Parent, _quest);
            player.Questing.QuestAbandoned += OnQuestAbandoned;
            player.Questing.QuestCompleted += OnQuestCompleted;
            RemoveThoughtsIfNecessary();
        }

        private void CreateQuest(IEntity Talker)
        {
            if (!(Talker is IPlayer player)) return;
            _quest?.Dispose();
            _quest = _questArchetype.Build(Parent, player);
            RemoveThoughtsIfNecessary();
            if (_quest != null)
            {
                Parent.AddComponent(new QuestThoughtsComponent(Parent, _quest.OpeningDialog));
                Parent.Removable = false;
                _quest.MissionDispose += () =>
                {
                    Parent.Removable = true;
                    Parent.AddComponent(new DisposeComponent(Parent, 1024));
                };
            }
            else
            {
                Parent.AddComponent(new ForgotQuestThoughtsComponent(Parent));
            }
        }

        private void RemoveThoughtsIfNecessary()
        {
            if (Parent.SearchComponent<ThoughtsComponent>() != null)
                Parent.RemoveComponent(Parent.SearchComponent<ThoughtsComponent>());
        }

        public override void Dispose()
        {
            if (_quest != null && _quest.Owner != null)
            {
                _quest.Owner.Questing.QuestAbandoned -= OnQuestAbandoned;
                _quest.Owner.Questing.QuestCompleted -= OnQuestCompleted;
            }

            Parent.RemoveComponent(_talk);
            Parent.ShowIcon(null);
            base.Dispose();
        }
    }
}