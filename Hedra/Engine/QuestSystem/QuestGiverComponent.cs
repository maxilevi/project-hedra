using System;
using Hedra.Components;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.EntitySystem;

namespace Hedra.Engine.QuestSystem
{
    public class QuestGiverComponent : Component<IHumanoid>
    {
        private readonly QuestObject _quest;
        private readonly TalkComponent _talk;
        
        public QuestGiverComponent(IHumanoid Parent, QuestObject Quest) : base(Parent)
        {
            if(Parent.SearchComponent<TalkComponent>() != null)
                throw new ArgumentException("There can only be 1 talk component");
            Parent.AddComponent(new QuestGiverThoughtsComponent(Parent));
            Parent.AddComponent(_talk = new TalkComponent(Parent));
            _talk.OnTalkingEnded += AddQuest;
            Parent.ShowIcon(CacheItem.AttentionIcon);
            _quest = Quest;
        }

        public override void Update()
        {
        }

        private void AddQuest(IEntity Interactee)
        {
            if(!(Interactee is IPlayer player) || (Parent.Position - player.Position).LengthSquared > 16 * 16) return;
            player.Questing.Start(_quest);
            /*player.ShowQuestDialog(Parent, _quest, () =>
            {
                _talk.Dispose();
                Dispose();
            });*/
        }

        public override void Dispose()
        {
            base.Dispose();
            Parent.ShowIcon(null);
        }
    }
}