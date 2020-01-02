using System;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.Localization;

namespace Hedra.Mission.Blocks
{
    public class WaitForTimeMission : MissionBlock
    {
        private readonly int _targetTime;
        private readonly float _speed;
        private bool _setup;

        public override bool IsCompleted
        {
            get
            {
                var isCompleted = Math.Abs(SkyManager.DayTime - _targetTime) < 100f;
                if(isCompleted) SkyManager.DaytimeSpeed = 1;
                return isCompleted;
            }
        }

        public WaitForTimeMission(int Target, float Speed = 1.0f)
        {
            _speed = Speed;
            _targetTime = Target;
        }
        
        public override void Setup()
        {
            _setup = true;
            SkyManager.PushTime();
            SkyManager.DaytimeSpeed = _speed;
        }

        public void Pop()
        {
            if(!_setup) return;
            SkyManager.PopTime();
        }

        public override QuestView BuildView()
        {
            return new ModelView(CacheManager.GetModel(CacheItem.ClockIcon));
        }

        public override bool HasLocation => false;
        public override string ShortDescription => Translations.Get("quest_wait_for_time_short");
        public override string Description => Translations.Get("quest_wait_for_time_desc");
        public override DialogObject DefaultOpeningDialog => default;
    }
}