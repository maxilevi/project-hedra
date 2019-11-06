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
        private int _targetSkyTime;
        private SoundType _soundType;
        private bool _restoreSoundPlayed;
        private bool _isInsideZone;
        private float _previousTime;
        private float _targetTime;
        private float _beforeSaveTime;
        private float _oldTime;
        private bool _shouldUpdateTime;

        public bool IsActive => _isInsideZone;
        
        public TimeHandler(int TargetSkyTime, SoundType Sound = SoundType.DarkSound)
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
            _previousTime = SkyManager.DayTime;
            if(_previousTime < 12000) SkyManager.DayTime += 24000;
            SkyManager.Enabled = false;
            _targetTime = _targetSkyTime;
            _shouldUpdateTime = true;
            SoundPlayer.PlayUISound(_soundType);
            _isInsideZone = true;
        }

        public void Remove()
        {
            if(!_isInsideZone) return;
            _targetTime = _previousTime;
            if(_previousTime < 12000) _targetTime += 24000;
            _shouldUpdateTime = true;
            SkyManager.Enabled = false;
            _isInsideZone = false;
        }
             
        private void HandleSky()
        {
            if(_shouldUpdateTime)
            {
                
                SkyManager.SetTime( Mathf.Lerp(SkyManager.DayTime, _targetTime, Time.DeltaTime * 2f) );
                if( Math.Abs(SkyManager.DayTime - _targetTime) < 10 )
                {
                    _shouldUpdateTime = false;
                    SkyManager.Enabled = true;
                    if( SkyManager.DayTime > 24000) SkyManager.DayTime -= 24000;
                }
            }
        }
        
        private void BeforeSave(object Invoker, EventArgs Args)
        {
            if(_isInsideZone)
            {
                _beforeSaveTime = SkyManager.DayTime;
                SkyManager.DayTime = _previousTime;
            }
            _oldTime = float.MaxValue;
            if (SkyManager.StackLength > 0)
            {
                _oldTime = SkyManager.PeekTime();
                SkyManager.PopTime();
            }
        }
        
        private void AfterSave(object Invoker, EventArgs Args)
        {
            if(_isInsideZone)
            {
                SkyManager.DayTime = _beforeSaveTime;
            }

            if (_oldTime != float.MaxValue)
            {
                SkyManager.DayTime = _oldTime;
                SkyManager.PushTime();
            }
        }

        public void Dispose()
        {
            GameManager.AfterSave -= AfterSave;
            GameManager.BeforeSave -= BeforeSave;
        }
    }
}