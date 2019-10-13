/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 09/01/2017
 * Time: 05:08 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.IO;
using Hedra.Engine.Management;
using Hedra.Engine.Scripting;
using Hedra.Engine.Sound;
using NVorbis;
using OpenToolkit.OpenAL;

namespace Hedra.Sound
{
    public delegate void OnSongEnd();
    public static class SoundtrackManager
    {
        private const string LibraryName = "Soundtrack.py";
        public static int MainTheme { get; private set; }
        public static int HostageSituation { get; private set; }
        public static int GraveyardChampion { get; private set; }
        public static int Rain { get; private set; }
        public static int VillageAmbient { get; private set; }
        public static int OnTheLam { get; private set; }
        private static Script _script;
        private static event OnSongEnd SongEnded;
        private static readonly object Lock = new object();
        private static readonly float[] Buffer = new float[176400 * 2];
        private static SoundSource _source;
        private static SoundBuffer _backBuffer;
        private static SoundBuffer _frontBuffer;
        private static SoundBuffer _usedBuffer;
        private static VorbisReader _reader;
        private static int _sampleRate;
        private static int _channels;
        private static bool _buildBuffers;
        private static int _receivedBytes;
        private static bool _loaded;
        /* State */
        private static int _currentActionSong;
        private static int _currentAmbientSong;
        private static bool _isPlayingAmbient;
        private static AL _al;

        public static void Load()
        {
            Load(new SoundSource(SoundPlayer.ListenerPosition));
            _al = AL.GetAPI();
        }

        public static void Load(SoundSource Source)
        {
            _source = Source;
            _loaded = true;
            SongEnded += OnSongEnd;
            LoadLibrary();
        }

        private static void OnSongEnd()
        {
            _script.Get("on_song_end").Invoke(_isPlayingAmbient, _currentActionSong, _currentAmbientSong);
        }
        
        public static void PlayAmbient()
        {
            if (!_loaded) return;
            _isPlayingAmbient = true;
            _currentAmbientSong = _script.Get("resume_ambient").Invoke<int>(_currentAmbientSong);
        }

        public static void PlayRepeating(int Index)
        {
            if (!_loaded) return;
            _isPlayingAmbient = false;
            _currentActionSong = _script.Get("resume_action").Invoke<int>(Index);
        }
        
        public static void Update()
        {
            if ( !_loaded) return;
            
            _source.Position = SoundPlayer.ListenerPosition;
            _source.Volume = FinalVolume;

            if (FinalVolume < 0.005f) return;

            if(_usedBuffer != null && !_source.IsPlaying && _receivedBytes > 0)
            {
                _al.SetSourceProperty(_source.Id, SourceInteger.Buffer, (int) _usedBuffer.Id);
                _al.SourcePlay(_source.Id);
                _buildBuffers = true;
            }
            
            if(_buildBuffers && _receivedBytes > -1 && _reader != null)
            {
                lock (Lock)
                {
                    _receivedBytes = _reader.ReadSamples(Buffer, 0, Buffer.Length);
                }

                if (_receivedBytes <= 0)
                {
                    SongEnded?.Invoke();
                    return;
                }
                var data = CastBuffer(Buffer, _receivedBytes);
                if(_usedBuffer == null || (_frontBuffer != null && _usedBuffer.Id == _frontBuffer.Id))
                {
                    _backBuffer?.Dispose();
                    _backBuffer = new SoundBuffer(data, SoundPlayer.GetSoundFormat(_channels, 16), _sampleRate);
                    _usedBuffer = _backBuffer;
                }
                else if(_usedBuffer.Id == _backBuffer.Id)
                {
                    _frontBuffer?.Dispose();
                    _frontBuffer = new SoundBuffer(data, SoundPlayer.GetSoundFormat(_channels, 16), _sampleRate);
                    _usedBuffer = _frontBuffer;
                }
                _buildBuffers = false;
            }
        }
        
        private static short[] CastBuffer(float[] Buffer, int Length)
        {
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
        
        /* Used from scripts (SoundtrackPlayer.py) */
        public static void PlayTrack(string Name)
        {
            if(!_loaded) return;
            _usedBuffer = null;
            _buildBuffers = true;

            var bytes = AssetManager.ReadBinary(Name, AssetManager.SoundResource);
            Stream stream = new MemoryStream(bytes);
            lock (Lock)
            {
                _receivedBytes = 0;
                _reader?.Dispose();
                _reader = new VorbisReader(stream, true);
                _sampleRate = _reader.SampleRate;
                _channels = _reader.Channels;
            }
        }
        
        private static float FinalVolume => Volume * PauseVolume * SongTransitionVolume;

        /* Used from scripts (SoundtrackPlayer.py) */
        public static float PauseVolume { get; set; }
        /* Used from scripts (SoundtrackPlayer.py) */
        public static float SongTransitionVolume { get; set; } = 1;

        public static float Volume { get; set; } = 0.2f;

        private static void LoadLibrary()
        {
            _script = Interpreter.GetScript(LibraryName);
            MainTheme = _script.Get<int>("MAIN_THEME");
            HostageSituation = _script.Get<int>("HOSTAGE_SITUATION");
            GraveyardChampion = _script.Get<int>("GRAVEYARD_CHAMPION");
            Rain = _script.Get<int>("RAIN");
            VillageAmbient = _script.Get<int>("VILLAGE_AMBIENT");
            OnTheLam = _script.Get<int>("ON_THE_LAM");
            _script.Get("soundtrack_setup").Invoke();
        }
    }
}

