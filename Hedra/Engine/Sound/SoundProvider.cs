using System;
using System.Collections.Generic;
using System.IO;
using Hedra.Core;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Sound;
using NVorbis;
using OpenToolkit.Mathematics;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace Hedra.Engine.Sound
{
    public class SoundProvider : ISoundProvider
    {
        private readonly Dictionary<string, SoundFamily> _soundFamilies;
        private readonly SoundItem[] _soundItems;
        private readonly SoundSource[] _soundSources;
        private AudioContext _audioContext;
        private bool _loaded;
        
        public Vector3 ListenerPosition { get; private set; }
        public float Volume { get; set; } = 0.4f;

        public SoundProvider()
        {
            _soundFamilies = new Dictionary<string, SoundFamily>();
            _soundItems = new SoundItem[8];
            _soundSources = new SoundSource[32];
        }

        public void Setup()
        {
            _audioContext = new AudioContext();
            Log.WriteLine("Generating a pool of sound sources...");
            for (var i = 0; i < _soundSources.Length; i++)
            {
                _soundSources[i] = new SoundSource(Vector3.Zero);
            }
            Log.WriteLine("Generating a pool of sound items...");
            for (var i = 0; i < _soundItems.Length; i++)
            {
                _soundItems[i] = new SoundItem(new SoundSource(Vector3.Zero));
            }
        }

        public void MarkAsReady()
        {            
            _loaded = true;
        }

        public void LoadSound(string Name, params string[] Names)
        {
            var family = new SoundFamily();
            for (var i = 0; i < Names.Length; i++)
            {
                if(!Names[i].EndsWith(".ogg"))
                    throw new ArgumentException("Only '.ogg' files are supported.");
                
                family.Add(new SoundBuffer(
                    LoadOgg(Names[i], out var channels, out var bits, out var rate),
                    GetSoundFormat(channels, bits),
                    rate
                ));
            }
            _soundFamilies[Name] = family;

        }

        public void Update(Vector3 Position)
        {
            if(!_loaded) return;
            ListenerPosition = Position;
        }

        public void PlaySound(string Sound, Vector3 Location, bool Looping = false, float Pitch = 1, float Gain = 1)
        {

            if(!_loaded || Sound == SoundType.None.ToString()) return;
            ListenerPosition = LocalPlayer.Instance.Position;

            Gain = Math.Max(Gain * (1-(ListenerPosition - Location).LengthFast / 128f) * Volume, 0);
            if(Gain <= 0 ) return;

            var source = GrabSource();
            if(source == null)
            {
                Log.WriteLine($"Could not play sound {Sound}");
                return;
            }
            source.Play(GetBuffer(Sound), Location, Pitch, Gain, Looping);            
        }

        public void PlaySoundWhile(string Sound, Func<bool> Lambda, Func<float> PitchLambda, Func<float> GainLambda)
        {
            if (!_loaded || Sound == SoundType.None.ToString()) return;
            var source = GrabSource();
            if(source == null)
            {
                Log.WriteLine($"Could not play sound {Sound}");
                return;
            }
            TaskScheduler.While(Lambda, delegate
            {
                if(source.IsPlaying) return;
                source.Play(GetBuffer(Sound), ListenerPosition, PitchLambda(), GainLambda(), false);
            });
        }

        private SoundSource GrabSource()
        {
            SoundSource source = null;
            for (var i = 0; i < _soundSources.Length; i++)
            {
                if (_soundSources[i].IsPlaying) continue;
                source = _soundSources[i];
                break;
            }
            return source;
        }

        public SoundBuffer GetBuffer(string Type)
        {
            return _soundFamilies[Type.ToString()].Get();
        }

        public SoundItem GetAvailableSource()
        {
            if (!_loaded) return null;

            for (var i = 0; i < _soundItems.Length; i++)
            {
                if (!_soundItems[i].Locked)
                {
                    _soundItems[i].Locked = true;
                    return _soundItems[i];
                }
            }
            return null;
        }

        public short[] LoadOgg(string File, out int Channels, out int Bits, out int Rate, out int Count)
        {
            return LoadOgg(File, out Channels, out Bits, out Rate, out _, out Count, 0, -1);
        }

        private short[] LoadOgg(string File, out int Channels, out int Bits, out int Rate)
        {
            return LoadOgg(File, out Channels, out Bits, out Rate, out _, out _, 0, -1);
        }
        
        public short[] LoadOgg(string File, out int Channels, out int Bits, out int Rate, out int BytesPerSecond, int Offset, int Length){
            return LoadOgg(File, out Channels, out Bits, out Rate, out BytesPerSecond, out _, Offset, Length);
        }

        private short[] LoadOgg(string File, out int Channels, out int Bits, out int Rate, out int BytesPerSecond, out int Count, int Offset, int Length)
        {
            var bytes = AssetManager.ReadPath(File, false);
            Stream stream = new MemoryStream(bytes);
            
            using(var reader = new VorbisReader(stream, true))
            {
                if (Length == -1)
                {
                    var secs = reader.TotalTime.TotalSeconds;
                    if (Math.Ceiling(secs) - secs < secs * 0.01)
                        Length = (int) Math.Ceiling(secs) * reader.SampleRate * sizeof(short);
                    else
                        Length = (int)Math.Ceiling(secs * reader.SampleRate * sizeof(short));
                }
                short[] data = new short[Length];
                float[] buffer = new float[Length];
                
                if(Offset != 0)
                {
                    float[] offsetBuffer = new float[Offset];
                    reader.ReadSamples(offsetBuffer, 0, Offset);
                }
                reader.ReadSamples(buffer, 0, Length);
                
                for (var i = 0; i < Length; i++)
                {
                    var temp = (int)( (short.MaxValue-1) * buffer[i]);
                    if (temp > short.MaxValue) temp = short.MaxValue;
                    else if (temp < short.MinValue) temp = short.MinValue;
                    data[i] = (short) temp;
                }
            
                    
                Channels = reader.Channels;
                Rate = reader.SampleRate;
                Bits = 16;
                BytesPerSecond = Rate * 4;
                Count = (int)Math.Round(reader.TotalTime.TotalSeconds) * BytesPerSecond;
                
                return data;
            }
        }

        public byte[] LoadWave(string File, out int Channels, out int Bits, out int Rate)
        {
            int byterate = 0;
            return LoadWave(File, out Channels, out Bits, out Rate, out byterate, 0, -1);
        }

        private byte[] LoadWave(string File, out int Channels, out int Bits, out int Rate, out int BytesPerSecond, int Offset, int Length)
        {
            
            byte[] bytes = AssetManager.ReadBinary(File, AssetManager.SoundResource);
            Stream stream = new MemoryStream(bytes);
            
            if (stream == null)
                throw new ArgumentNullException("stream");

            using (BinaryReader reader = new BinaryReader(stream))
            {
                // RIFF header
                string signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                int riffChunckSize = reader.ReadInt32();

                string format = new string(reader.ReadChars(4));
                if (format != "WAVE")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                // WAVE header
                string formatSignature = new string(reader.ReadChars(4));
                if (formatSignature != "fmt ")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int formatChunkSize = reader.ReadInt32();
                int audioFormat = reader.ReadInt16();
                int numChannels = reader.ReadInt16();
                int sampleRate = reader.ReadInt32();
                int byteRate = reader.ReadInt32();
                int blockAlign = reader.ReadInt16();
                int bitsPerSample = reader.ReadInt16();

                string dataSignature = new string(reader.ReadChars(4));
                if (dataSignature != "data")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int dataChunkSize = reader.ReadInt32();

                Channels = numChannels;
                Bits = bitsPerSample;
                Rate = sampleRate;
                BytesPerSecond = byteRate;
                //offset
                reader.ReadBytes(Offset);
                
                return Length == -1 
                    ? reader.ReadBytes((int) reader.BaseStream.Length-Offset) 
                    : reader.ReadBytes(Length);
            }
        }

        public ALFormat GetSoundFormat(int Channels, int Bits)
        {
            switch (Channels)
            {
                case 1: return Bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                case 2: return Bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                default: throw new NotSupportedException("The specified sound format is not supported.");
            }
        }
    }
}