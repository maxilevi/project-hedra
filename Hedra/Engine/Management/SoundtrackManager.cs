/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 09/01/2017
 * Time: 05:08 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Sound;
using Hedra.Engine.Scenes;
using Hedra.Engine.Player;
using System.IO;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using NVorbis;

namespace Hedra.Engine.Management
{
	/// <summary>
	/// Description of SoundtrackManager.
	/// </summary>
	public static class SoundtrackManager
	{
		public static SoundSource Source;
		public static string[] TrackNames;
		private static int TrackIndex = 0;
		private static SoundBuffer BackBuffer, FrontBuffer;
		private static SoundBuffer UsedBuffer;
		private static bool BuildBuffers = true;
		private static int ReceivedBytes = 0;
		private static VorbisReader Reader;
		private static float[] Buffer = new float[176400 * 2];//??
		public static bool MainThemePlaying = false;
		private static bool Loaded = false;
		private static int OldTrackIndex;
		private static int OldReceivedBytes;
		private static bool SleepTime = false;
		private static Timer Ticker = new Timer(6f);
		private static float TargetVolume = 0f;
		private static int AmbientIndex = -1;
		public const int VillageIndex = 0, MainThemeIndex = 1, TotalAmbient = 2;
        private static bool WasInMenu = false;
		
		
		public static void Load(){
			Source = new SoundSource(SoundManager.ListenerPosition);

			TrackNames = new string[9];
            TrackNames[0] = "Sounds/VillageAmbient.ogg";
            TrackNames[1] = "Sounds/MainTheme.ogg";
            TrackNames[2] = "Sounds/ForestAmbient.ogg";
			TrackNames[3] = "Sounds/Song0.ogg";
			TrackNames[4] = "Sounds/Song1.ogg";
			TrackNames[5] = "Sounds/Song2.ogg";
			TrackNames[6] = "Sounds/Song3.ogg";
			TrackNames[7] = "Sounds/Song4.ogg";
			TrackNames[8] = "Sounds/Song5.ogg";
			
			for(int i = 0; i < TrackNames.Length; i++){
				if(TrackNames[i] == null)
					throw new ArgumentException("Array cannot contain null values");
			}
			
			Volume = .4f;
			Loaded = true;
		}
		
		public static void PlayAmbient(int Index){
			if(TrackIndex < TotalAmbient) return;
			
			OldTrackIndex = TrackIndex;
			OldReceivedBytes = ReceivedBytes;
			
			AmbientIndex = Index;
			PlayingAmbient = true;
		}
		
		public static void Update(){

            if (SceneManager.Game.InMenuWorld)//In the starting menu so roll the music!
            {
                TargetVolume = Volume;
                TrackIndex = MainThemeIndex-1;

                if (Math.Abs(TargetVolume - FinalVolume) > 0.005f)
                    FinalVolume = Mathf.Lerp(FinalVolume, TargetVolume, (float)Time.unScaledDeltaTime);

                if (!WasInMenu)
                {
                    ReceivedBytes = 0;
                }
                WasInMenu = true;
                goto PLAY;
            }
            else
            {
                if (!WasInMenu)
                {

                }

                WasInMenu = false;
            }

            if ( !Loaded || (GameSettings.Paused || SceneManager.Game.IsLoading || TrackNames.Length == 0) && !MainThemePlaying) return;
			
			Source.Position = SoundManager.ListenerPosition;
			
			if(!SleepTime){
				if(PlayingAmbient && TrackIndex != AmbientIndex)
					TargetVolume = 0f;
				else
					TargetVolume = Volume;
				
				if( Math.Abs(TargetVolume - FinalVolume) > 0.005f)
					FinalVolume = Mathf.Lerp(FinalVolume, TargetVolume, (float) Time.deltaTime);
				else if(PlayingAmbient && TrackIndex != AmbientIndex){
					ReceivedBytes = 0;
					TrackIndex = AmbientIndex;
					Ticker.Reset();
				}
			
			}else{
				TargetVolume = 0f;
			}

            PLAY:

            if (FinalVolume < 0.005f) return;

			if( ReceivedBytes == 0 && Ticker.Tick()){
				SleepTime = false;
				//ChangeTrack
				if(!PlayingAmbient){
					if(TrackIndex < TrackNames.Length-1)
						TrackIndex++;
					else
						TrackIndex = TotalAmbient;//Dont play ambient songs
				}
				UsedBuffer = null;
				BuildBuffers = true;
				ReceivedBytes = -1;
				if(Reader != null) Reader.Dispose();
				
				byte[] Bytes = AssetManager.ReadBinary(TrackNames[TrackIndex], AssetManager.DataFile2);
				Stream stream = new MemoryStream(Bytes);
				Reader = new VorbisReader(stream, true);

				return;
			}
			
			if(UsedBuffer != null && !Source.IsPlaying && ReceivedBytes != 0){
			    AL.Source(Source.ID, ALSourcei.Buffer, (int) UsedBuffer.ID);
			    AL.SourcePlay(Source.ID);
                BuildBuffers = true;
			}
			
			if(BuildBuffers  && ReceivedBytes != 0){
				ReceivedBytes = Reader.ReadSamples(Buffer, 0, Buffer.Length);
				if(ReceivedBytes == 0){
					SleepTime = true;
					Ticker.Reset();
					if(PlayingAmbient)
						PlayingAmbient = false;
				}
				short[] Data = CastBuffer(Buffer, ReceivedBytes);
				//Default buffer
				if(UsedBuffer == null || (FrontBuffer != null && UsedBuffer.ID == FrontBuffer.ID) ){
					if(BackBuffer != null)
						BackBuffer.Dispose();
					BackBuffer = new SoundBuffer(SoundManager.GetSoundFormat(Reader.Channels, 16), Data, Reader.SampleRate);
					UsedBuffer = BackBuffer;
				} else if(UsedBuffer.ID == BackBuffer.ID){
					if(FrontBuffer != null)
						FrontBuffer.Dispose();
					FrontBuffer = new SoundBuffer(SoundManager.GetSoundFormat(Reader.Channels, 16), Data, Reader.SampleRate);
					UsedBuffer = FrontBuffer;
				}
				BuildBuffers = false;
			}
		 }
		
		private static short[] CastBuffer(float[] buffer, int length){
			short[] Data = new short[length];
			for (int i = 0; i < length; i++)
            {
				var temp = (int)( (short.MaxValue-1) * buffer[i]);
                if (temp > short.MaxValue) temp = short.MaxValue;
                else if (temp < short.MinValue) temp = short.MinValue;
                Data[i] = (short) temp;
            }
			return Data;
		}
		
		private static bool m_PlayingAmbient = false;
		private static bool PlayingAmbient{
			get{ return m_PlayingAmbient; }
			set{
				if(m_PlayingAmbient && !value){
					ReceivedBytes = OldReceivedBytes;
					TrackIndex = OldTrackIndex;
					
					OldTrackIndex = -1;
					OldReceivedBytes = -1;
				}
				m_PlayingAmbient = value;
			}
		}
		
		private static float m_FinalVolume;
		public static float FinalVolume{
			get{ return m_FinalVolume; }
			set{
				if(Loaded)
					AL.Source(Source.ID, ALSourcef.Gain, value);
				m_FinalVolume = value;
			}
		}

		private static float m_Volume;
		public static float Volume{
			get{ return m_Volume; }
			set{
				m_Volume = value;
			}
		}
		
		
	}
}

