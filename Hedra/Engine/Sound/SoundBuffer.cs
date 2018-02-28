﻿/*
 * Author: Zaphyk
 * Date: 02/03/2016
 * Time: 01:26 a.m.
 *
 */
using System;
using OpenTK.Audio.OpenAL;
using OpenTK.Audio;
using Hedra.Engine.Management;

namespace Hedra.Engine.Sound
{
	/// <summary>
	/// Description of SoundBuffer.
	/// </summary>
	public class SoundBuffer : IDisposable
	{
		public uint ID;

	    public SoundBuffer(ALFormat Format, byte[] Data, int SampleRate){
			AL.GenBuffers(1, out ID);
			AL.BufferData( (int) ID, Format, Data, Data.Length, SampleRate);
			DisposeManager.Add(this);
		}
		
		public SoundBuffer(ALFormat Format, short[] Data, int SampleRate){
			AL.GenBuffers(1, out ID);
			AL.BufferData( (int) ID, Format, Data, Data.Length * sizeof(short), SampleRate);
			DisposeManager.Add(this);
		}
		
		public SoundBuffer(){
			DisposeManager.Add(this);
		}
		
		public void Dispose(){
			DisposeManager.Remove(this);
			AL.DeleteBuffers(1, ref ID);
		}
	}
}
