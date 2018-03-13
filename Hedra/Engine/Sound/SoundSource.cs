/*
 * Author: Zaphyk
 * Date: 02/03/2016
 * Time: 02:13 a.m.
 *
 */

using System;
using System.Net;
using OpenTK;
using OpenTK.Audio.OpenAL;

namespace Hedra.Engine.Sound
{
	/// <summary>
	/// Description of SoundSource.
	/// </summary>
	public class SoundSource
	{
		public uint ID;
		
		public SoundSource(Vector3 Position){ 

            AL.GenSources(1, out ID);
			AL.Source(ID, ALSourcef.Gain, 1);
			AL.Source(ID, ALSourcef.Pitch,  1);
			AL.Source(ID, ALSource3f.Position, ref Position);
        }

	    public void Stop()
	    {
	        AL.SourceStop(ID);
        }

        private void Play(SoundBuffer Buffer)
        {
            var position = SoundManager.ListenerPosition;
            AL.Listener(ALListener3f.Position, ref position);

            AL.Source(ID, ALSourcei.Buffer, (int) Buffer.ID);
            AL.SourcePlay(ID);

        }
		
		public void Play(SoundBuffer Buffer, Vector3 Location, float Pitch, float Gain, bool Loop){

            AL.Source(ID, ALSourcef.Pitch, Pitch);
			AL.Source(ID, ALSourcef.Gain, Gain );
			AL.Source(ID, ALSource3f.Position, ref Location);
		    AL.Source(ID, ALSourceb.Looping, Loop);

            this.Stop();
            this.Play(Buffer);

        }

	    public bool IsPlaying
	    {
	        get
	        {
	            //if (System.Threading.Thread.CurrentThread.ManagedThreadId != Hedra.MainThreadId)
	            //    throw new Exception("Playing ! Duude");

                ALSourceState State;
	            int i;
	            AL.GetSource(ID, ALGetSourcei.SourceState, out i);
	            State = (ALSourceState)i;

	            return (State == ALSourceState.Playing);
	        }
	    }

        private Vector3 _position;
		public Vector3 Position{
			get{return this._position;}
			set{
                if(value == this._position) return;
				AL.Source(ID, ALSource3f.Position, ref value);

                this._position = value;
			}
		}

	    private float _volume;
	    public float Volume
	    {
	        get { return this._volume; }
	        set
	        {
	            if (System.Threading.Thread.CurrentThread.ManagedThreadId != Hedra.MainThreadId)
	                throw new Exception("Duude");

	            if (value == this._volume) return;
                AL.Source(ID, ALSourcef.Gain, value);

                this._volume = value;
	        }
	    }
	}
}
