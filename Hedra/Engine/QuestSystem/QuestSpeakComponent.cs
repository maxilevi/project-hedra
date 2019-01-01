using Hedra.Components;
using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;

namespace Hedra.Engine.QuestSystem
{
    public class QuestSpeakComponent : Component<IHumanoid>
    {
        public bool Spoke { get; private set; }
        private readonly TalkComponent _talk;
        
        public QuestSpeakComponent(IHumanoid Parent) : base(Parent)
        {
            _talk = new TalkComponent(Parent);
            _talk.OnTalkingEnded += T => Spoke = true;
            Parent.AddComponent(_talk);
        }

        public override void Update()
        { 
        }

        public override void Dispose()
        {
            _talk.Dispose();
        }
    }
}