using System;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Game;
using Hedra.Sound;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class TimeHandler
    {
        private readonly SoundType _soundType;
        private readonly int _targetSkyTime;
        private int _beforeSaveTime;
        private int _previousTime;
        private bool _restoreSoundPlayed;
        private SkySettings _settings;

        public TimeHandler(int TargetSkyTime, SoundType Sound = SoundType.None)
        {
            _targetSkyTime = TargetSkyTime;
            _soundType = Sound;
            GameManager.AfterSave += AfterSave;
            GameManager.BeforeSave += BeforeSave;
        }

        public bool IsActive { get; private set; }

        public void Update()
        {
            HandleSky();
        }

        public void Apply()
        {
            if (IsActive) return;
            SkyManager.PushTime();
            SkyManager.Enabled = false;
            SkyManager.DayTime = _targetSkyTime;
            SoundPlayer.PlayUISound(_soundType);
            IsActive = true;
        }

        public void Remove()
        {
            if (!IsActive) return;
            SkyManager.PopTime();
            IsActive = false;
        }

        private void HandleSky()
        {
            /*
            if (Math.Abs(float.MaxValue - _targetTime) > 0.005f)
            {
                SkyManager.SetTime(Mathf.Lerp(SkyManager.DayTime, _targetTime, (float)Time.DeltaTime * 2f));
                if (Math.Abs(SkyManager.DayTime - _targetTime) < 10)
                {
                    if (SkyManager.DayTime > 24000) SkyManager.DayTime -= 24000;
                }
            }*/
        }

        private void BeforeSave(object Invoker, EventArgs Args)
        {
            if (IsActive)
                _settings = SkyManager.PopTime();
        }

        private void AfterSave(object Invoker, EventArgs Args)
        {
            if (IsActive)
                SkyManager.PushTime(_settings);
            _settings = null;
        }

        public void Dispose()
        {
            GameManager.AfterSave -= AfterSave;
            GameManager.BeforeSave -= BeforeSave;
        }
    }
}