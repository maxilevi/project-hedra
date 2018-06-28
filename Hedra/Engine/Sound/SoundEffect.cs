/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 25/04/2016
 * Time: 05:01 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Hedra.Engine.Sound
{
	/// <summary>
	/// Description of SoundEffect.
	/// </summary>
	internal struct SoundEffect
	{
		public SoundType Type;
		public bool RandomPitch;
		public bool RandomVolume;
		public bool Initialized;
		
		public SoundEffect(SoundType Type, bool RandomPitch, bool RandomVolume, float Pitch, float Volume)
		{
			this.Type = Type;
			this.RandomPitch = RandomPitch;
			this.RandomVolume = RandomVolume;
			//this.m_Pitch = Pitch;
			//this.m_Volume = Volume;
			this.Initialized = true;
		}
		/*
		private float m_Pitch;
		public float Pitch{
			get{ return (RandomPitch) ? (float) SoundManager.Rng.NextDouble() : m_Pitch;}
			set{ m_Pitch = value; }
		}
		
		private float m_Volume;
		public float Volume{
			get{ return (RandomVolume) ? (float) SoundManager.Rng.NextDouble() : m_Volume;}
			set{ m_Volume = value; }
		}*/
	}
}
