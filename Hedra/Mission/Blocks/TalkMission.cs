using Hedra.Components;
using Hedra.Engine.Localization;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.Engine.QuestSystem;
using Hedra.EntitySystem;
using Hedra.Localization;
using System.Numerics;

namespace Hedra.Mission.Blocks
{
    public class TalkMission : MissionBlock
    {
        public event OnTalkEventHandler OnTalk;
        public IHumanoid Humanoid { get; set; }
        private readonly DialogObject _dialog;
        private TalkComponent _talk;
        private bool _isCompleted;

        public TalkMission(DialogObject Dialog)
        {
            _dialog = Dialog;
        }

        public override void Setup()
        {
            _talk = new TalkComponent(Humanoid);
            _talk.OnTalkingStarted += OnTalkingStarted;
            _talk.OnTalkingEnded += OnTalkingEnded;
            _talk.AutoRemove = true;
            Humanoid.AddComponent(_talk);
        }

        protected virtual void OnTalkingStarted(IEntity Talker)
        {
            var thoughts = new QuestThoughtsComponent(Humanoid, _dialog);
            Humanoid.AddComponent(thoughts);
            OnTalk?.Invoke(Talker);
        }

        protected virtual void OnTalkingEnded(IEntity Talker)
        {
            _isCompleted = true;
        }

        public void AddDialogLine(string Text)
        {
            _talk.AddDialogLine(Translation.Default(Text));
        }

        public override DialogObject DefaultOpeningDialog => default(DialogObject);

        public override void Cleanup()
        {
            base.Cleanup();
            Humanoid.RemoveComponent<TalkComponent>();
            if(Humanoid.SearchComponent<ThoughtsComponent>() != null)
                Humanoid.RemoveComponent<ThoughtsComponent>();
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
    }
}