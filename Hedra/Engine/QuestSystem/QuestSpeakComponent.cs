using Hedra.Components;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.QuestSystem.Designs;
using Hedra.EntitySystem;

namespace Hedra.Engine.QuestSystem
{
    public class QuestSpeakComponent : QuestComponent
    {
        public event OnTalkEventHandler Spoke;
        private readonly TalkComponent _talk;
        private readonly QuestThoughtsComponent _thoughts;
        
        public QuestSpeakComponent(IHumanoid Parent, QuestObject Object, QuestDesign Design) : base(Parent)
        {
            Parent.AddComponent(
                _thoughts = new QuestThoughtsComponent(
                    Parent,
                    Design.ThoughtsKeyword,
                    Design.GetThoughtsParameters(Object)
                )
            );
            Parent.AddComponent(_talk = new TalkComponent(Parent));
            _talk.OnTalkingEnded += T =>
            {
                Spoke?.Invoke(T);
                Parent.RemoveComponent(this);
            };
        }

        public override void Update()
        { 
        }

        public override void Dispose()
        {
            Parent.RemoveComponent(_talk);
            Parent.RemoveComponent(_thoughts);
            base.Dispose();
        }
    }
}