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
	    public const int VillageIndex = 0;
	    public const int MainThemeIndex = 1;
        public const int LoopableSongsStart = 2;

        private static readonly float[] Buffer = new float[176400 * 2];
	    private static readonly Timer Ticker = new Timer(6f);
        private static int _trackIndex = -1;
		private static SoundBuffer _backBuffer;
	    private static SoundBuffer _frontBuffer;
	    private static SoundBuffer _usedBuffer;
		private static bool _buildBuffers = true;
		private static int _receivedBytes;
		private static VorbisReader _reader;
		private static bool _loaded;
		private static int _oldTrackIndex;
		private static int _oldReceivedBytes;
		private static bool _sleepTime;
		private static float _targetVolume;
	    private static bool _repeatItself;


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
			
			for(var i = 0; i < TrackNames.Length; i++){
				if(TrackNames[i] == null)
					throw new ArgumentException("Array cannot contain null values");
			}
			
			Volume = .4f;
			_loaded = true;
		}
		
		public static void PlayTrack(int Index, bool RepeatItself = false)
		{
		    _repeatItself = RepeatItself;
            if(Index != _trackIndex) { 
		        _trackIndex = Index;
                SoundtrackManager.StartCurrentSong();
            }
		}

	    private static void NextTrack()
	    {
	        if (_repeatItself) return;
	        if (_trackIndex < TrackNames.Length - 1) _trackIndex++;
	        else _trackIndex = LoopableSongsStart + 1;
	    }

	    private static void StartCurrentSong()
	    {
	        _sleepTime = false;
	        _usedBuffer = null;
	        _buildBuffers = true;
	        _receivedBytes = -1;
	        _reader?.Dispose();

	        byte[] bytes = AssetManager.ReadBinary(TrackNames[_trackIndex], AssetManager.DataFile2);
	        Stream stream = new MemoryStream(bytes);
	        _reader = new VorbisReader(stream, true);
        }
		
		public static void Update(){

            if ( !_loaded || (GameSettings.Paused && !GameManager.InStartMenu) || GameManager.IsLoading || TrackNames.Length == 0 || _trackIndex < 0) return;
			
			Source.Position = SoundManager.ListenerPosition;
			
			if(!_sleepTime){
				_targetVolume = Volume;		
				if( Math.Abs(_targetVolume - FinalVolume) > 0.005f)
					FinalVolume = Mathf.Lerp(FinalVolume, _targetVolume, (float) Time.unScaledDeltaTime);		
			}else{
				_targetVolume = 0f;
			}

            PLAY:

            if (FinalVolume < 0.005f) return;

			if( _receivedBytes == 0 && Ticker.Tick())
			{
                SoundtrackManager.NextTrack();
			    SoundtrackManager.StartCurrentSong();
                return;
			}
			
			if(_usedBuffer != null && !Source.IsPlaying && _receivedBytes != 0){
			    AL.Source(Source.ID, ALSourcei.Buffer, (int) _usedBuffer.ID);
			    AL.SourcePlay(Source.ID);
                _buildBuffers = true;
			}
			
			if(_buildBuffers  && _receivedBytes != 0){
				_receivedBytes = _reader.ReadSamples(Buffer, 0, Buffer.Length);
				if(_receivedBytes == 0){
					_sleepTime = true;
					Ticker.Reset();
					if(PlayingAmbient)
						PlayingAmbient = false;
				}
				short[] data = CastBuffer(Buffer, _receivedBytes);
				//Default buffer
				if(_usedBuffer == null || (_frontBuffer != null && _usedBuffer.ID == _frontBuffer.ID) ){
				    _backBuffer?.Dispose();
				    _backBuffer = new SoundBuffer(SoundManager.GetSoundFormat(_reader.Channels, 16), data, _reader.SampleRate);
					_usedBuffer = _backBuffer;
				} else if(_usedBuffer.ID == _backBuffer.ID){
				    _frontBuffer?.Dispose();
				    _frontBuffer = new SoundBuffer(SoundManager.GetSoundFormat(_reader.Channels, 16), data, _reader.SampleRate);
					_usedBuffer = _frontBuffer;
				}
				_buildBuffers = false;
			}
		 }
		
		private static short[] CastBuffer(float[] Buffer, int Length){
			short[] data = new short[Length];
			for (int i = 0; i < Length; i++)
            {
				var temp = (int)( (short.MaxValue-1) * Buffer[i]);
                if (temp > short.MaxValue) temp = short.MaxValue;
                else if (temp < short.MinValue) temp = short.MinValue;
                data[i] = (short) temp;
            }
			return data;
		}
		
		private static bool _playingAmbient;
		private static bool PlayingAmbient{
			get{ return _playingAmbient; }
			set{
				if(_playingAmbient && !value){
					_receivedBytes = _oldReceivedBytes;
					_trackIndex = _oldTrackIndex;
					
					_oldTrackIndex = -1;
					_oldReceivedBytes = -1;
				}
				_playingAmbient = value;
			}
		}
		
		private static float _finalVolume;
		public static float FinalVolume{
			get{ return _finalVolume; }
			set{
				if(_loaded)
					AL.Source(Source.ID, ALSourcef.Gain, value);
				_finalVolume = value;
			}
		}

	    public static float Volume { get; set; }
	}
}

