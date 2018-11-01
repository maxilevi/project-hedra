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
using Hedra.Engine.Game;
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
        public const int RainIndex = 2;
        public const int GraveyardChampion = 3;
        public const int HostageSituation = 4;
        public const int OnTheLam = 5;

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
        private static int _loopableSongsStart;

        public static void Load()
        {
            Load(new SoundSource(SoundManager.ListenerPosition));
        }

        public static void Load(SoundSource Source)
        {
            SoundtrackManager.Source = Source;
            var forestTrack = "Sounds/Soundtrack/ForestAmbient.ogg";
            TrackNames = new string[]
            {
                "Sounds/Soundtrack/VillageAmbient.ogg",
                "Sounds/Soundtrack/MainTheme.ogg",
                "Sounds/Soundtrack/Rain.ogg",
                "Sounds/Soundtrack/GraveyardChampion.ogg",
                "Sounds/Soundtrack/HostageSituation.ogg",
                "Sounds/Soundtrack/OnTheLam.ogg",
                // LoopableSongs
                forestTrack,
                // Forest should always be first
                "Sounds/Soundtrack/CardinalCity.ogg",
                "Sounds/Soundtrack/ThroughTheGrasslands.ogg",
                "Sounds/Soundtrack/BreathOfDay.ogg",
                "Sounds/Soundtrack/TheVillage.ogg",
                "Sounds/Soundtrack/AdventurersMinuet.ogg"
            };
            _loopableSongsStart = Array.IndexOf(TrackNames, forestTrack);
            ShuffleSongs();

            for (var i = 0; i < TrackNames.Length; i++)
            {
                if(TrackNames[i] == null)
                    throw new ArgumentException("Array cannot contain null values");
            }
            
            Volume = .2f;
            _loaded = true;
        }

        private static void ShuffleSongs()
        {
            var n = TrackNames.Length;
            while (n > _loopableSongsStart+1)
            {
                var k = Utils.Rng.Next(_loopableSongsStart+1, n--);
                var temp = TrackNames[n];
                TrackNames[n] = TrackNames[k];
                TrackNames[k] = temp;
            }
        }

        public static void PlayAmbient()
        {
            PlayTrack(_loopableSongsStart);
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
            else _trackIndex = _loopableSongsStart + 1;
        }

        private static void StartCurrentSong()
        {
            if(!_loaded) return;
            if(_previousIndex != TrackIndex)// Song is looping, no interpolation
                _sleepTime = true;
            _usedBuffer = null;
            _buildBuffers = true;
            _receivedBytes = -1;
            _reader?.Dispose();

            byte[] bytes = AssetManager.ReadBinary(TrackNames[_trackIndex], AssetManager.DataFile2);
            Stream stream = new MemoryStream(bytes);
            _reader = new VorbisReader(stream, true);
        }
        
        public static void Update()
        {
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
            FinalVolume = Mathf.Lerp(FinalVolume, _targetVolume, Time.IndependantDeltaTime);

            PLAY:

            if (FinalVolume < 0.005f) return;

            if( _receivedBytes == 0 && (Ticker.Tick() || _repeatItself) )
            {
                SoundtrackManager.NextTrack();
                SoundtrackManager.StartCurrentSong();
                return;
            }
            
            if(_usedBuffer != null && !Source.IsPlaying && _receivedBytes != 0){
                AL.Source(Source.Id, ALSourcei.Buffer, (int) _usedBuffer.ID);
                AL.SourcePlay(Source.Id);
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
                    _backBuffer = new SoundBuffer(data, SoundManager.GetSoundFormat(_reader.Channels, 16), _reader.SampleRate);
                    _usedBuffer = _backBuffer;
                } else if(_usedBuffer.ID == _backBuffer.ID){
                    _frontBuffer?.Dispose();
                    _frontBuffer = new SoundBuffer(data, SoundManager.GetSoundFormat(_reader.Channels, 16), _reader.SampleRate);
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
                    AL.Source(Source.Id, ALSourcef.Gain, value);
                _finalVolume = value;
            }
        }

        public static float Volume { get; set; }
    }
}

