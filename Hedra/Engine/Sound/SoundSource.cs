/*
 * Author: Zaphyk
 * Date: 02/03/2016
 * Time: 02:13 a.m.
 *
 */

using System.Numerics;
using Silk.NET.OpenAL;

namespace Hedra.Engine.Sound
{
    /// <summary>
    ///     Description of SoundSource.
    /// </summary>
    public class SoundSource
    {
        private readonly AL _al;
        public readonly uint Id;
        private bool _initialized;
        private Vector3 _position;
        private float _volume;

        public unsafe SoundSource(Vector3 Position)
        {
            _al = AL.GetApi();
            var id = 0u;
            _al.GenSources(1, &id);
            _al.SetSourceProperty(Id, SourceFloat.Gain, 1);
            _al.SetSourceProperty(Id, SourceFloat.Pitch, 1);
            _al.SetSourceProperty(Id, SourceVector3.Position, Position);
            Id = id;
        }

        public bool IsPlaying
        {
            get
            {
                _al.GetSourceProperty(Id, GetSourceInteger.SourceState, out var i);
                var state = (SourceState)i;

                return state == SourceState.Playing;
            }
        }

        public Vector3 Position
        {
            get => _position;
            set
            {
                if (value == _position) return;
                unsafe
                {
                    _al.SetSourceProperty(Id, SourceVector3.Position, (float*)&value);
                }

                _position = value;
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

        public void Stop()
        {
            _al.SourceStop(Id);
        }

        public void Play(SoundBuffer Buffer)
        {
            _al.SetSourceProperty(Id, SourceInteger.Buffer, (int)Buffer.Id);
            var position = Vector3.Zero;
            _al.SetSourceProperty(Id, SourceVector3.Position, position);
            _al.SourcePlay(Id);
        }

        public void Play(SoundBuffer Buffer, Vector3 Location, float Pitch, float Gain, bool Loop)
        {
            _al.SetSourceProperty(Id, SourceFloat.Pitch, Pitch);
            _al.SetSourceProperty(Id, SourceFloat.Gain, Gain);
            _al.SetSourceProperty(Id, SourceBoolean.Looping, Loop);

            Stop();
            Play(Buffer);
        }
    }
}