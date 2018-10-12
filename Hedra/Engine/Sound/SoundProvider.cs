using System;
using System.IO;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using NVorbis;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace Hedra.Engine.Sound
{
    public class SoundProvider : ISoundProvider
    {
		private readonly SoundBuffer[] _soundBuffers;
		private readonly SoundItem[] _soundItems;
		private readonly SoundSource[] _soundSources;
	    private AudioContext _audioContext;
	    private bool _loaded;
	    
	    public Vector3 ListenerPosition { get; private set; }
		public float Volume { get; set; } = 0.4f;

		public SoundProvider()
		{
			_soundBuffers = new SoundBuffer[(int)SoundType.MaxSounds];
			_soundItems = new SoundItem[8];
			_soundSources = new SoundSource[32];
		}

        public void Load()
        {
			_audioContext = new AudioContext();
            Log.WriteLine("Generating a pool of sound sources...");
            for (int i = 0; i < _soundSources.Length; i++)
            {
                _soundSources[i] = new SoundSource(Vector3.Zero);
            }
            Log.WriteLine("Generating a pool of sound items...");
            for (int i = 0; i < _soundItems.Length; i++)
            {
				_soundItems[i] = new SoundItem(new SoundSource(Vector3.Zero));
			}
            Log.WriteLine("Loading sounds...");
            TaskManager.Parallel(delegate
            {
	            LoadSound(SoundType.ButtonClick, "Sounds/HoverButton.ogg");
	            LoadSound(SoundType.WaterSplash, "Sounds/WaterSplash.ogg");
	            LoadSound(SoundType.OnOff, "Sounds/OnOff.ogg");
	            LoadSound(SoundType.SwooshSound, "Sounds/Swoosh.ogg");
	            LoadSound(SoundType.HitSound, "Sounds/Hit.ogg");
	            LoadSound(SoundType.NotificationSound, "Sounds/ItemCollect.ogg");
	            LoadSound(SoundType.ArrowHit, "Sounds/Hit.ogg");
	            LoadSound(SoundType.BowSound, "Sounds/Bow.ogg");
	            LoadSound(SoundType.DarkSound, "Sounds/DarkSound.ogg");
	            LoadSound(SoundType.SlashSound, "Sounds/Slash.ogg");
	            LoadSound(SoundType.Jump, "Sounds/Jump.ogg");
	            LoadSound(SoundType.TransactionSound, "Sounds/Money.ogg");
	            LoadSound(SoundType.FoodEat, "Sounds/Eat.ogg");
	            _soundBuffers[(int) SoundType.FoodEaten] = _soundBuffers[(int) SoundType.NotificationSound];
	            LoadSound(SoundType.HorseRun, "Sounds/Horse.ogg");
	            LoadSound(SoundType.Fireplace, "Sounds/Fireplace.ogg");
	            LoadSound(SoundType.HumanRun, "Sounds/Run.ogg");
	            LoadSound(SoundType.HitGround, "Sounds/HitGround.ogg");
	            LoadSound(SoundType.Dodge, "Sounds/Roll.ogg");
	            LoadSound(SoundType.LongSwoosh, "Sounds/LongSwoosh.ogg");
	            LoadSound(SoundType.GlassBreak, "Sounds/GlassBreak.ogg");
	            LoadSound(SoundType.GlassBreakInverted, "Sounds/GlassBreakInverted.ogg");
	            LoadSound(SoundType.HumanSleep, "Sounds/HumanSleep.ogg");
	            LoadSound(SoundType.TalkSound, "Sounds/ItemCollect.ogg");
	            LoadSound(SoundType.GroundQuake, "Sounds/GroundQuake.ogg");
	            LoadSound(SoundType.SpitSound, "Sounds/Bow.ogg");
	            LoadSound(SoundType.GorillaGrowl, "Sounds/GorillaGrowl.ogg");
	            LoadSound(SoundType.PreparingAttack, "Sounds/PreparingAttack.ogg");
                LoadSound(SoundType.River, "Sounds/River.ogg");
	            LoadSound(SoundType.BoatMove, "Sounds/BoatMove.ogg");
                _loaded = true;
                Log.WriteLine("Finished loading sounds.");
            });
        }

	    private void LoadSound(SoundType Type, string Name)
	    {
	        _soundBuffers[(int)Type] = 
		        new SoundBuffer(
			        LoadOgg(Name, out var channels, out var bits, out var rate),
			        GetSoundFormat(channels, bits),
			        rate
			        );
        }

        public void Update(Vector3 Position)
        {
			if(!_loaded) return;
            ListenerPosition = Position;
        }

        public void PlaySound(SoundType Sound, Vector3 Location, bool Looping = false, float Pitch = 1, float Gain = 1)
        {

            if(!_loaded) return;
            ListenerPosition = LocalPlayer.Instance.Position;

		    Gain = Math.Max(Gain * (1-(ListenerPosition - Location).LengthFast / 128f) * Volume, 0);
            if(Gain <= 0 ) return;

	        var source = GrabSource();
			if(source == null)
			{
				Log.WriteLine($"Could not play sound {Sound}");
				return;
			}
			source.Play(_soundBuffers[ (int) Sound], Location, Pitch, Gain, Looping);			
		}

		public void PlaySoundWhile(SoundType Sound, Func<bool> Lambda, Func<float> PitchLambda, Func<float> GainLambda)
		{
			if (!_loaded) return;
			var source = GrabSource();
			if(source == null)
			{
				Log.WriteLine($"Could not play sound {Sound}");
				return;
			}
            TaskManager.While(Lambda, delegate
            {
	            if(source.IsPlaying) return;
	            source.Play(_soundBuffers[(int)Sound], ListenerPosition, PitchLambda(), GainLambda(), false);
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

	    public SoundBuffer GetBuffer(SoundType Type)
	    {
	        return _soundBuffers[(int)Type];
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
			
			byte[] bytes = AssetManager.ReadBinary(File, AssetManager.DataFile2);
			Stream stream = new MemoryStream(bytes);
			
			using(VorbisReader reader = new VorbisReader(stream, true))
			{

			    if (Length == -1)
			    {
			        var secs = reader.TotalTime.TotalSeconds;
                    if (secs < 1)
			            Length = (int) Math.Ceiling(reader.TotalTime.TotalSeconds * reader.SampleRate * sizeof(short));
                    else
                        Length = (int) Math.Ceiling(reader.TotalTime.TotalSeconds) * reader.SampleRate * sizeof(short);
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
			
			byte[] bytes = AssetManager.ReadBinary(File, AssetManager.DataFile2);
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