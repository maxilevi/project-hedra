using Hedra.Components;
using Hedra.Engine.Localization;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.Engine.QuestSystem;
using Hedra.EntitySystem;
using Hedra.Localization;
using OpenTK;

namespace Hedra.Mission.Blocks
{
    public class TalkMission : MissionBlock
    {
        public event OnTalkEventHandler OnTalk;
        public IHumanoid Humanoid { get; set; }
        private readonly string _keyword;
        private readonly object[] _arguments;
        private TalkComponent _talk;
        private bool _isCompleted;

        protected TalkMission(DialogObject Dialog)
        {
            _keyword = Dialog.Key;
            _arguments = Dialog.Params;   
        }

        public TalkMission(string Keyword, params object[] Arguments)
            : this(new DialogObject { Key = Keyword, Params = Arguments })
        {
        }

        public override void Setup()
        {
            _talk = new TalkComponent(Humanoid);
            _talk.OnTalkingStarted += OnTalkingStarted;
            _talk.OnTalkingEnded += Talker => _isCompleted = true;
            Humanoid.AddComponent(_talk);
        }

        private void OnTalkingStarted(IEntity Talker)
        {
            var thoughts = new QuestThoughtsComponent(Humanoid, _keyword, _arguments);
            Humanoid.AddComponent(thoughts);
            OnTalk?.Invoke(Talker);
        }
        

        public void AddDialogLine(string Text)
        {
            _talk.AddDialogLine(Translation.Default(Text));
        }
        
        public override void Dispose()
        {
            Humanoid.RemoveComponent(Humanoid.SearchComponent<TalkComponent>());
        }

        public override QuestView BuildView()
        {
            return new EntityView(Humanoid.Model);
        }

        public override bool IsCompleted => _isCompleted;
        public override bool HasLocation => true;
        public override Vector3 Location => Humanoid.Position;
        public override string ShortDescription => Translations.Get("quest_speak_short", Humanoid.Name);
        public override string Description => Translations.Get("quest_speak_description", Humanoid.Name);
        
        protected struct DialogObject
        {
            public string Key { get; set; }
            public object[] Params { get; set; }
        }
    }
}