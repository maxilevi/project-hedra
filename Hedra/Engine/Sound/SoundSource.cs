/*
 * Author: Zaphyk
 * Date: 02/03/2016
 * Time: 02:13 a.m.
 *
 */

using System;
using OpenTK;
using OpenTK.Audio.OpenAL;

namespace Hedra.Engine.Sound
{
	/// <summary>
	/// Description of SoundSource.
	/// </summary>
	public class SoundSource
	{
		public readonly uint Id;		
		private Vector3 _position;
		private float _volume;
		
		public SoundSource(Vector3 Position)
		{
            AL.GenSources(1, out Id);
			AL.Source(Id, ALSourcef.Gain, 1);
			AL.Source(Id, ALSourcef.Pitch,  1);
			AL.Source(Id, ALSource3f.Position, ref Position);
        }

	    public void Stop()
	    {
	        AL.SourceStop(Id);
        }

        private void Play(SoundBuffer Buffer)
        {
            var position = SoundManager.ListenerPosition;
            AL.Listener(ALListener3f.Position, ref position);

            AL.Source(Id, ALSourcei.Buffer, (int) Buffer.ID);
            AL.SourcePlay(Id);
        }
		
		public void Play(SoundBuffer Buffer, Vector3 Location, float Pitch, float Gain, bool Loop)
		{
            AL.Source(Id, ALSourcef.Pitch, Pitch);
			AL.Source(Id, ALSourcef.Gain, Gain );
			AL.Source(Id, ALSource3f.Position, ref Location);
		    AL.Source(Id, ALSourceb.Looping, Loop);

            this.Stop();
            this.Play(Buffer);
        }

	    public bool IsPlaying
	    {
	        get
	        {
		        AL.GetSource(Id, ALGetSourcei.SourceState, out var i);
	            var state = (ALSourceState)i;

	            return (state == ALSourceState.Playing);
	        }
	    }

		public Vector3 Position
		{
			get => this._position;
			set{
                if(value == this._position) return;
				AL.Source(Id, ALSource3f.Position, ref value);

                this._position = value;
			}
		}

	    public float Volume
	    {
	        get => this._volume;
		    set
	        {
	            if (System.Threading.Thread.CurrentThread.ManagedThreadId != Hedra.MainThreadId)
	                throw new Exception("Duude");

	            if (value == this._volume) return;
                AL.Source(Id, ALSourcef.Gain, value);

                this._volume = value;
	        }
	    }
	}
}
