using System;
using Hedra.Core;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Game;
using Hedra.Engine.Rendering.Particles;
using Hedra.Game;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class GraveyardAmbientHandler : IDisposable
    {
        private readonly ParticleSystem _particles;
        private readonly int _passedTime = 0;
        private bool _restoreSoundPlayed;
        private readonly Graveyard _parent;
        private bool _inCementery;
        private float _cementeryTime;
        private float _targetCementeryTime;
        private float _oldCementeryTime;
        private float _oldTime;
        private bool _shouldUpdateTime;
        
        public GraveyardAmbientHandler(Graveyard Parent)
        {
            _parent = Parent;
            _particles = new ParticleSystem();
            GameManager.AfterSave += AfterSave;
            GameManager.BeforeSave += BeforeSave;
        }

        public void Update()
        {
            this.HandleSky();
            this.HandleParticles();
        }
             
        private void HandleSky()
        {
            var wasInCementery = _inCementery;
            
            _inCementery = (_parent.Position.Xz - GameManager.Player.Position.Xz).LengthSquared <
                           _parent.Radius * _parent.Radius * .5f * .5f && !_parent.Completed;
            
            if(_inCementery && !wasInCementery)
            {
                _cementeryTime = SkyManager.DayTime;
                if(_cementeryTime < 12000) SkyManager.DayTime += 24000;
                SkyManager.Enabled = false;
                _targetCementeryTime = GraveyardDesign.GraveyardSkyTime;
                _shouldUpdateTime = true;
                SoundPlayer.PlayUISound(SoundType.DarkSound);
            }
            else if (!_inCementery && wasInCementery)
            {
                _targetCementeryTime = _cementeryTime;
                if(this._cementeryTime < 12000) _targetCementeryTime += 24000;
                _shouldUpdateTime = true;
                SkyManager.Enabled = false;
            }

            if(_shouldUpdateTime)
            {
                
                SkyManager.SetTime( Mathf.Lerp(SkyManager.DayTime, _targetCementeryTime, Time.DeltaTime * 2f) );
                if( Math.Abs(SkyManager.DayTime - _targetCementeryTime) < 10 )
                {
                    _shouldUpdateTime = false;
                    SkyManager.Enabled = true;
                    if( SkyManager.DayTime > 24000) SkyManager.DayTime -= 24000;
                }
            }
        }
        
        private void HandleParticles()
        {
            if (_parent.Completed && !_restoreSoundPlayed)
            {
                _restoreSoundPlayed = true;
                SoundPlayer.PlaySound(SoundType.DarkSound, GameManager.Player.Position);

            }

            if (!_parent.Completed &&  (_parent.Position - GameManager.Player.Position).Xz.LengthSquared 
                < _parent.Radius * _parent.Radius)
            {
            
                if(_passedTime % 2 == 0){
                    _particles.Color = Particle3D.AshColor;
                    _particles.VariateUniformly = false;
                    _particles.Position = GameManager.Player.Position + Vector3.UnitY * 1f;
                    _particles.Scale = Vector3.One * .85f;
                    _particles.ScaleErrorMargin = new Vector3(.05f,.05f,.05f);
                    _particles.Direction = Vector3.UnitY * 0f;
                    _particles.ParticleLifetime = 2f;
                    _particles.GravityEffect = -0.000f;
                    _particles.PositionErrorMargin = new Vector3(64f, 12f, 64f);
                    _particles.Grayscale = true;
                    
                    _particles.Emit();
                }
            }
        }
            
        private void BeforeSave(object Invoker, EventArgs Args)
        {
            if(_inCementery)
            {
                _oldCementeryTime = SkyManager.DayTime;
                SkyManager.DayTime = _cementeryTime;
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
            if(_inCementery)
            {
                SkyManager.DayTime = _oldCementeryTime;
            }

            if (_oldTime != float.MaxValue)
            {
                SkyManager.DayTime = _oldTime;
                SkyManager.PushTime();
            }
        }

        public void Dispose()
        {
            _particles.Dispose();
            GameManager.AfterSave -= AfterSave;
            GameManager.BeforeSave -= BeforeSave;
        }
    }
}