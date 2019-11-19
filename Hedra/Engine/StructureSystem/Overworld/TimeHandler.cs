using System;
using Hedra.Core;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Rendering.Particles;
using Hedra.Game;
using Hedra.Numerics;
using Hedra.Sound;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class TimeHandler
    {
        private readonly int _targetSkyTime;
        private readonly SoundType _soundType;
        private bool _restoreSoundPlayed;
        private bool _isInsideZone;
        private int _beforeSaveTime;
        private int _previousTime;
        private SkySettings _settings;

        public bool IsActive => _isInsideZone;
        
        public TimeHandler(int TargetSkyTime, SoundType Sound = SoundType.None)
        {
            _targetSkyTime = TargetSkyTime;
            _soundType = Sound;
            GameManager.AfterSave += AfterSave;
            GameManager.BeforeSave += BeforeSave;
        }

        public void Update()
        {
            this.HandleSky();
        }

        public void Apply()
        {
            if(_isInsideZone) return;
            SkyManager.PushTime();
            SkyManager.Enabled = false;
            SkyManager.DayTime = _targetSkyTime;
            SoundPlayer.PlayUISound(_soundType);
            _isInsideZone = true;
        }

        public void Remove()
        {
            if(!_isInsideZone) return;
            SkyManager.PopTime();
            _isInsideZone = false;
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
            if (_isInsideZone)
                _settings = SkyManager.PopTime();
        }
        
        private void AfterSave(object Invoker, EventArgs Args)
        {
            if(_isInsideZone)
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