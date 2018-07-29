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
using System.Linq;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using NVorbis;

namespace Hedra.Engine.Management
{
	/// <summary>
	/// Description of SoundtrackManager.
	/// </summary>
	internal static class SoundtrackManager
	{
		public static SoundSource Source;
		public static string[] TrackNames;
	    public const int VillageIndex = 0;
	    public const int MainThemeIndex = 1;
	    public const int RainIndex = 2;
        public const int LoopableSongsStart = 3;

	    public static int TrackIndex => _trackIndex;
	    public static bool RepeatTrack => _repeatItself;
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
	    private static int _previousIndex;

        public static void Load()
        {
			Source = new SoundSource(SoundManager.ListenerPosition);

			TrackNames = new string[13];
            TrackNames[0] = "Sounds/Soundtrack/VillageAmbient.ogg";
            TrackNames[1] = "Sounds/Soundtrack/MainTheme.ogg";
            TrackNames[2] = "Sounds/Soundtrack/Rain.ogg";
            // LoopableSongs
            TrackNames[3] = "Sounds/Soundtrack/ForestAmbient.ogg";
            // Forest should always be first
			TrackNames[4] = "Sounds/Soundtrack/Song0.ogg";
			TrackNames[5] = "Sounds/Soundtrack/Song1.ogg";
			TrackNames[6] = "Sounds/Soundtrack/Song2.ogg";
			TrackNames[7] = "Sounds/Soundtrack/Song3.ogg";
			TrackNames[8] = "Sounds/Soundtrack/Song4.ogg";
			TrackNames[9] = "Sounds/Soundtrack/Song5.ogg";
            TrackNames[10] = "Sounds/Soundtrack/CardinalCity.ogg";
            TrackNames[11] = "Sounds/Soundtrack/ThroughTheGrasslands.ogg";
            TrackNames[12] = "Sounds/Soundtrack/BreathOfDay.ogg";
            ShuffleSongs();

            for (var i = 0; i < TrackNames.Length; i++){
				if(TrackNames[i] == null)
					throw new ArgumentException("Array cannot contain null values");
			}
			
			Volume = .4f;
			_loaded = true;
		}

	    private static void ShuffleSongs()
	    {
	        int n = TrackNames.Length;
	        while (n > LoopableSongsStart+1)
	        {
	            int k = Utils.Rng.Next(LoopableSongsStart+1, n--);
	            string temp = TrackNames[n];
	            TrackNames[n] = TrackNames[k];
	            TrackNames[k] = temp;
	        }
        }
		
		public static void PlayTrack(int Index, bool RepeatItself = false)
		{
            if(!_loaded) return;
		    _repeatItself = RepeatItself;
            if(Index != _trackIndex)
            {
                _previousIndex = _trackIndex;
                _trackIndex = Index;
                SoundtrackManager.StartCurrentSong();
            }
		}

	    private static void NextTrack()
	    {
	        _previousIndex = TrackIndex;
            if (_repeatItself) return;
            if (_trackIndex < TrackNames.Length - 1) _trackIndex++;
	        else _trackIndex = LoopableSongsStart + 1;
	    }

	    private static void StartCurrentSong()
	    {
		    if(!_loaded) return;
            if(_previousIndex != TrackIndex)//Song is looping, no interpolation
                _sleepTime = true;
	        _usedBuffer = null;
	        _buildBuffers = true;
	        _receivedBytes = -1;
	        _reader?.Dispose();

	        byte[] bytes = AssetManager.ReadBinary(TrackNames[_trackIndex], AssetManager.DataFile2);
	        Stream stream = new MemoryStream(bytes);
	        _reader = new VorbisReader(stream, true);
        }
		
		public static void Update(){

            if ( !_loaded || GameSettings.Paused && !GameManager.InStartMenu || GameManager.IsLoading || TrackNames.Length == 0 || _trackIndex < 0) return;
			
			Source.Position = SoundManager.ListenerPosition;

		    if (_sleepTime)
		    {
		        _targetVolume = 0;
		    }
		    else
		    {
		        _targetVolume = Volume;
            }
		    if (Math.Abs(_targetVolume - FinalVolume) < 0.005f && _sleepTime)
		    {
		        _targetVolume = Volume;
		        _sleepTime = false;
		    }
            FinalVolume = Mathf.Lerp(FinalVolume, _targetVolume, (float)Time.IndependantDeltaTime);

            PLAY:

            if (FinalVolume < 0.005f) return;

			if( _receivedBytes == 0 && (Ticker.Tick() || _repeatItself) )
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

