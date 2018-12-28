using System;
using Hedra.Components;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Player;
using Hedra.EntitySystem;

namespace Hedra.Engine.QuestSystem
{
    public class QuestGiverComponent : Component<IHumanoid>
    {
        private readonly BaseQuest _quest;
        private readonly TalkComponent _talk;
        
        public QuestGiverComponent(IHumanoid Parent, BaseQuest Quest) : base(Parent)
        {
            if(Parent.SearchComponent<TalkComponent>() != null)
                throw new ArgumentException("There can only be 1 talk component");
            //Parent.AddComponent(_talk = new TalkComponent(Parent, Quest.Design.OpeningLine));
            _talk.OnTalk += AddQuest;
        }

        public override void Update()
        {
        }

        private void AddQuest(IEntity Interactee)
        {
            if(!(Interactee is IPlayer player)) return;
            player.Questing.Start(_quest);
        }
    }
}