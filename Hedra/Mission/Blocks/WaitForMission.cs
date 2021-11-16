using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.EntitySystem;
using Hedra.Localization;

namespace Hedra.Mission.Blocks
{
    public class WaitForMission : MissionBlock
    {
        private readonly IHumanoid _humanoid;
        private readonly Timer _timer;
        private bool _started;

        public WaitForMission(IHumanoid Humanoid, float Time)
        {
            _humanoid = Humanoid;
            _timer = new Timer(Time)
            {
                AutoReset = false
            };
        }

        public override bool IsCompleted => _timer.Ready;

        public override bool HasLocation => true;
        public override Vector3 Location => _humanoid.Position;
        public override string Description => Translations.Get("quest_wait_for_description", _humanoid.Name);
        public override string ShortDescription => Translations.Get("quest_wait_for_short", _humanoid.Name);
        public override DialogObject DefaultOpeningDialog => default;

        public override void Setup()
        {
            _started = true;
        }

        public override void Update()
        {
            base.Update();
            if (_started && _timer.Tick()) Owner.Questing.Trigger();
        }

        public override QuestView BuildView()
        {
            return new EntityView(_humanoid.Model);
        }
    }
}