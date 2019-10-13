/*
 * Author: Zaphyk
 * Date: 02/03/2016
 * Time: 02:13 a.m.
 *
 */

using System;
using Hedra.Sound;
using OpenToolkit.Mathematics;
using OpenToolkit.OpenAL;

namespace Hedra.Engine.Sound
{
    /// <summary>
    /// Description of SoundSource.
    /// </summary>
    public class SoundSource
    {
        private readonly AL _al;
        public readonly uint Id;        
        private Vector3 _position;
        private float _volume;
        private bool _initialized;
        
        public unsafe SoundSource(Vector3 Position)
        {
            _al = AL.GetAPI();
            var id = 0u;
            _al.GenSources(1, &id);
            _al.SetSourceProperty(Id, SourceFloat.Gain, 1);
            _al.SetSourceProperty(Id, SourceFloat.Pitch,  1);
            _al.SetSourceProperty(Id, SourceVector3.Position, Position);
            Id = id;
        }

        public void Stop()
        {
            _al.SourceStop(Id);
        }

        private void Play(SoundBuffer Buffer)
        {
            var position = SoundPlayer.ListenerPosition;
            _al.SetListenerProperty(ListenerVector3.Position, position);

            _al.SetSourceProperty(Id, SourceInteger.Buffer, (int) Buffer.Id);
            _al.SourcePlay(Id);
        }
        
        public void Play(SoundBuffer Buffer, Vector3 Location, float Pitch, float Gain, bool Loop)
        {
            _al.SetSourceProperty(Id, SourceFloat.Pitch, Pitch);
            _al.SetSourceProperty(Id, SourceFloat.Gain, Gain );
            _al.SetSourceProperty(Id, SourceVector3.Position, Location);
            // TODO _al.SetSourceProperty(Id, SourceBoolean.Looping, Loop ? 1 : 0);

            this.Stop();
            this.Play(Buffer);
        }

        public bool IsPlaying
        {
            get
            {
                _al.GetSourceProperty(Id, GetSourceInteger.SourceState, out var i);
                var state = (SourceState)i;

                return (state == SourceState.Playing);
            }
        }

        public Vector3 Position
        {
            get => this._position;
            set{
                if(value == this._position) return;
                _al.SetSourceProperty(Id, SourceVector3.Position, value);

                this._position = value;
            }
        }

        public float Volume
        {
            get => _volume;
            set
            {
                _al.SetSourceProperty(Id, SourceFloat.Gain, _volume = value);
                _initialized = true;
            }
        }
    }
}
