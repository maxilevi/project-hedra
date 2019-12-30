using System;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.Localization;

namespace Hedra.Mission.Blocks
{
    public class WaitForTime : MissionBlock
    {
        private readonly int _targetTime;
        private readonly float _speed;

        public override bool IsCompleted => Math.Abs(SkyManager.DayTime - _targetTime) < 0.005f;

        public WaitForTime(int Target, float Speed = 1.0f)
        {
            _speed = Speed;
            _targetTime = Target;
        }
        
        public override void Setup()
        {
            SkyManager.PushTime();
            SkyManager.DayTime = _targetTime;
            SkyManager.DaytimeSpeed = _speed;
        }

        public void Pop()
        {
            SkyManager.PopTime();
        }

        public override QuestView BuildView()
        {
            return new ModelView();
        }

        public override bool HasLocation => false;
        public override string ShortDescription => Translations.Get("");
        public override string Description => Translations.Get("");
        public override DialogObject DefaultOpeningDialog => default;
    }
}