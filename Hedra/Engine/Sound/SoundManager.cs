/*
 * Author: Zaphyk
 * Date: 02/03/2016
 * Time: 01:19 a.m.
 *
 */
using System;
using System.IO;
using OpenTK;
using OpenTK.Audio.OpenAL;
using OpenTK.Audio;
using System.Collections.Generic;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using NVorbis.Ogg;
using NVorbis;
using OpenTK.Graphics.ES10;


namespace Hedra.Engine.Sound
{
	/// <summary>
	/// Description of SoundManager.
	/// </summary>
	public static class SoundManager
	{
		public static AudioContext AudioContext;
	    public static float Volume = 0.4f;
        public static Vector3 ListenerPosition { get; private set; }

	    private static bool _loaded = false;
        private static readonly SoundBuffer[] SoundBuffers = new SoundBuffer[(int)SoundType.MaxSounds];
        public static readonly SoundItem[] SoundItems = new SoundItem[8];
	    private static readonly SoundSource[] SoundSources = new SoundSource[32];

        public static void Load()
        {
       
			AudioContext = new AudioContext();
            Log.WriteLine("Generating a pool of sound sources...");
            for (int i = 0; i < SoundSources.Length; i++)
            {
                SoundSources[i] = new SoundSource(Vector3.Zero);
            }
            Log.WriteLine("Generating a pool of sound items...");
            for (int i = 0; i < SoundItems.Length; i++){
				SoundItems[i] = new SoundItem(new SoundSource(Vector3.Zero));
			}
            Log.WriteLine("Loading sounds...");
            int Channels, Bits, Rate;
            short[] ShortData;
			byte[] Data = SoundManager.LoadWave("Sounds/HoverButton.wav", out Channels, out Bits, out Rate);
			SoundBuffers[(int) SoundType.ButtonClick] = new SoundBuffer(GetSoundFormat(Channels, Bits), Data, Rate);

			Data = SoundManager.LoadWave("Sounds/WaterSplash.wav", out Channels, out Bits, out Rate);
			SoundBuffers[(int) SoundType.WaterSplash] = new SoundBuffer(GetSoundFormat(Channels, Bits), Data, Rate);
			
			Data = SoundManager.LoadWave("Sounds/OnOff.wav", out Channels, out Bits, out Rate);
			SoundBuffers[(int) SoundType.OnOff] = new SoundBuffer(GetSoundFormat(Channels, Bits), Data, Rate);
			
			Data = SoundManager.LoadWave("Sounds/Swoosh.wav", out Channels, out Bits, out Rate);
			SoundBuffers[(int) SoundType.SwooshSound] = new SoundBuffer(GetSoundFormat(Channels, Bits), Data, Rate);

            ShortData = SoundManager.LoadOgg("Sounds/Hit.ogg", out Channels, out Bits, out Rate);
            SoundBuffers[(int) SoundType.HitSound] = new SoundBuffer(GetSoundFormat(Channels, Bits), ShortData, Rate);
			
			Data = SoundManager.LoadWave("Sounds/ItemCollect.wav", out Channels, out Bits, out Rate);
			SoundBuffers[(int) SoundType.NotificationSound] = new SoundBuffer(GetSoundFormat(Channels, Bits), Data, Rate);
			
			//Data = SoundManager.LoadWave("Sounds/ItemCollect.wav", out Channels, out Bits, out Rate);
		    SoundBuffers[(int) SoundType.FoodEaten] = SoundBuffers[(int) SoundType.NotificationSound];//new SoundBuffer(GetSoundFormat(Channels, Bits), Data, Rate);

            ShortData = SoundManager.LoadOgg("Sounds/Hit.ogg", out Channels, out Bits, out Rate);
            SoundBuffers[(int) SoundType.ArrowHit] = new SoundBuffer(GetSoundFormat(Channels, Bits), ShortData, Rate);
			
			Data = SoundManager.LoadWave("Sounds/Bow.wav", out Channels, out Bits, out Rate);
			SoundBuffers[(int) SoundType.BowSound] = new SoundBuffer(GetSoundFormat(Channels, Bits), Data, Rate);
			
			Data = SoundManager.LoadWave("Sounds/DarkSound.wav", out Channels, out Bits, out Rate);
			SoundBuffers[(int) SoundType.DarkSound] = new SoundBuffer(GetSoundFormat(Channels, Bits), Data, Rate);

            ShortData = SoundManager.LoadOgg("Sounds/Slash.ogg", out Channels, out Bits, out Rate);
			SoundBuffers[(int) SoundType.SlashSound] = new SoundBuffer(GetSoundFormat(Channels, Bits), ShortData, Rate);
			
			Data = SoundManager.LoadWave("Sounds/Footstep/footstep_01.wav", out Channels, out Bits, out Rate);
			SoundBuffers[(int) SoundType.FootStep] = new SoundBuffer(GetSoundFormat(Channels, Bits), Data, Rate);
			
			Data = SoundManager.LoadWave("Sounds/Jump.wav", out Channels, out Bits, out Rate);
			SoundBuffers[(int) SoundType.Jump  ] = new SoundBuffer(GetSoundFormat(Channels, Bits), Data, Rate);
			
			Data = SoundManager.LoadWave("Sounds/Money.wav", out Channels, out Bits, out Rate);
			SoundBuffers[(int) SoundType.TransactionSound  ] = new SoundBuffer(GetSoundFormat(Channels, Bits), Data, Rate);

            Data = SoundManager.LoadWave("Sounds/Eat.wav", out Channels, out Bits, out Rate);
            SoundBuffers[(int)SoundType.FoodEat] = new SoundBuffer(GetSoundFormat(Channels, Bits), Data, Rate);

		    ShortData = SoundManager.LoadOgg("Sounds/Horse.ogg", out Channels, out Bits, out Rate);
		    SoundBuffers[(int)SoundType.HorseRun] = new SoundBuffer(GetSoundFormat(Channels, Bits), ShortData, Rate);

		    ShortData = SoundManager.LoadOgg("Sounds/Fireplace.ogg", out Channels, out Bits, out Rate);
		    SoundBuffers[(int)SoundType.Fireplace] = new SoundBuffer(GetSoundFormat(Channels, Bits), ShortData, Rate);

		    Data = SoundManager.LoadWave("Sounds/Run.wav", out Channels, out Bits, out Rate);
		    SoundBuffers[(int)SoundType.HumanRun] = new SoundBuffer(GetSoundFormat(Channels, Bits), Data, Rate);

		    Data = SoundManager.LoadWave("Sounds/HitGround.wav", out Channels, out Bits, out Rate);
		    SoundBuffers[(int)SoundType.HitGround] = new SoundBuffer(GetSoundFormat(Channels, Bits), Data, Rate);

		    ShortData = SoundManager.LoadOgg("Sounds/Roll.ogg", out Channels, out Bits, out Rate);
		    SoundBuffers[(int)SoundType.Dodge] = new SoundBuffer(GetSoundFormat(Channels, Bits), ShortData, Rate);

            ShortData = SoundManager.LoadOgg("Sounds/LongSwoosh.ogg", out Channels, out Bits, out Rate);
            SoundBuffers[(int)SoundType.LongSwoosh] = new SoundBuffer(GetSoundFormat(Channels, Bits), ShortData, Rate);

            ShortData = SoundManager.LoadOgg("Sounds/GlassBreak.ogg", out Channels, out Bits, out Rate);
            SoundBuffers[(int)SoundType.GlassBreak] = new SoundBuffer(GetSoundFormat(Channels, Bits), ShortData, Rate);

            ShortData = SoundManager.LoadOgg("Sounds/GlassBreakInverted.ogg", out Channels, out Bits, out Rate);
            SoundBuffers[(int)SoundType.GlassBreakInverted] = new SoundBuffer(GetSoundFormat(Channels, Bits), ShortData, Rate);

            ShortData = SoundManager.LoadOgg("Sounds/HumanSleep.ogg", out Channels, out Bits, out Rate);
            SoundBuffers[(int)SoundType.HumanSleep] = new SoundBuffer(GetSoundFormat(Channels, Bits), ShortData, Rate);

            _loaded = true;
		}

        public static void Update(Vector3 Position){
			if(!_loaded) return;

            ListenerPosition = Position;
            //ALError error = AL.GetError();
            //if (error != ALError.NoError)
            //    Log.WriteResult(false, error.ToString());
        }

        public static void PlaySound(SoundType Sound, Vector3 Location, bool Looping = false, float Pitch = 1, float Gain = 1)
        {

            if(!_loaded) return;
            ListenerPosition = LocalPlayer.Instance.Position;

            Gain *= Volume;
		    Gain = Math.Max(Gain - Math.Max(Math.Min(1, (ListenerPosition - Location).LengthFast / 256), 0), 0);
            if(Gain <= 0 ) return;

            SoundType type = Sound;
			
			SoundSource source = null;
			for(int i = 0; i < SoundSources.Length; i++){
			    if (!SoundSources[i].IsPlaying)
			    {
			        source = SoundSources[i];
                    break;
			    }
			}
			if(source == null){
				Log.WriteLine("Could not play sound "+ Sound.ToString());
				return;
			}
 
			source.Play(SoundBuffers[ (int) type], Location, Pitch, Gain, Looping);
			
		}
		
		public static void PlayUISound(SoundType Sound, float Pitch = 1, float Gain = 1){
            if (!_loaded) return;
            PlaySound(Sound, ListenerPosition, false, Pitch, Gain);
		}
		
		public static void PlaySoundWithVariation(SoundType Sound, Vector3 Location, float BasePitch = 1f, float BaseGain = 1f){
            if (!_loaded) return;
            PlaySound(Sound, Location, false, BasePitch + Utils.Rng.NextFloat() * .2f - .1f, BaseGain + Utils.Rng.NextFloat() * .2f - .1f);
		}

	    public static SoundBuffer GetBuffer(SoundType Type)
	    {
	        return SoundBuffers[(int)Type];
	    }

        public static SoundItem GetAvailableSource()
        {
            if (!_loaded) return null;

            for (int i = 0; i < SoundItems.Length; i++)
	        {
	            if (!SoundItems[i].Locked)
	            {
	                SoundItems[i].Locked = true;
	                return SoundItems[i];
	            }
	        }
	        return null;
	    }

        #region OGG
        public static short[] LoadOgg(string file, out int channels, out int bits, out int rate, out int count)
		{
			int byterate = 0;
			return LoadOgg(file, out channels, out bits, out rate, out byterate, out count, 0, -1);
		}
		
		public static short[] LoadOgg(string file, out int channels, out int bits, out int rate)
		{
			int byterate = 0, count = 0;
			return LoadOgg(file, out channels, out bits, out rate, out byterate, out count, 0, -1);
		}
		
		public static short[] LoadOgg(string file, out int channels, out int bits, out int rate, out int bytes_per_second, int offset, int length){
			int count = 0;
			return LoadOgg(file, out channels, out bits, out rate, out bytes_per_second, out count, offset, length);
		}
		
		public static short[] LoadOgg(string file, out int channels, out int bits, out int rate, out int bytes_per_second, out int count, int offset, int length){
			
			byte[] Bytes = AssetManager.ReadBinary(file, AssetManager.DataFile2);
			Stream stream = new MemoryStream(Bytes);
			
			using(VorbisReader Reader = new VorbisReader(stream, true)){
			
				if(length == -1) length = (int) ( Math.Ceiling(Reader.TotalTime.TotalSeconds) * Reader.SampleRate * 2);
				
				short[] Data = new short[length];
				float[] buffer = new float[length];
				
				if(offset != 0){
					float[] offsetBuffer = new float[offset];
					Reader.ReadSamples(offsetBuffer, 0, offset);
				}
				Reader.ReadSamples(buffer, 0, length);
				
				for (int i = 0; i < length; i++)
	            {
					var temp = (int)( (short.MaxValue-1) * buffer[i]);
	                if (temp > short.MaxValue) temp = short.MaxValue;
	                else if (temp < short.MinValue) temp = short.MinValue;
	                Data[i] = (short) temp;
	            }
			
					
				channels = Reader.Channels;
				rate = Reader.SampleRate;
				bits = 16;
				bytes_per_second = rate * 4;
				count = (int)Math.Round(Reader.TotalTime.TotalSeconds) * bytes_per_second;
				
				return Data;
			}
		}
		#endregion
		
		#region WAV
		public static byte[] LoadWave(string file, out int channels, out int bits, out int rate)
		{
			int byterate = 0;
			return LoadWave(file, out channels, out bits, out rate, out byterate, 0, -1);
		}
		
		public static byte[] LoadWave(string file, out int channels, out int bits, out int rate, out int bytes_per_second, int offset, int length)
        {
			
			byte[] Bytes = AssetManager.ReadBinary(file, AssetManager.DataFile2);
			Stream stream = new MemoryStream(Bytes);
			
            if (stream == null)
                throw new ArgumentNullException("stream");

            using (BinaryReader reader = new BinaryReader(stream))
            {
                // RIFF header
                string signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                int riff_chunck_size = reader.ReadInt32();

                string format = new string(reader.ReadChars(4));
                if (format != "WAVE")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                // WAVE header
                string format_signature = new string(reader.ReadChars(4));
                if (format_signature != "fmt ")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int format_chunk_size = reader.ReadInt32();
                int audio_format = reader.ReadInt16();
                int num_channels = reader.ReadInt16();
                int sample_rate = reader.ReadInt32();
                int byte_rate = reader.ReadInt32();
                int block_align = reader.ReadInt16();
                int bits_per_sample = reader.ReadInt16();

                string data_signature = new string(reader.ReadChars(4));
                if (data_signature != "data")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int data_chunk_size = reader.ReadInt32();

                channels = num_channels;
                bits = bits_per_sample;
                rate = sample_rate;
                bytes_per_second = byte_rate;
                //offset
                reader.ReadBytes(offset);
                
                if(length == -1)
               		return reader.ReadBytes((int) reader.BaseStream.Length-offset);
                else
                	return reader.ReadBytes(length);
            }
        }

        public static ALFormat GetSoundFormat(int channels, int bits)
        {
            switch (channels)
            {
                case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                default: throw new NotSupportedException("The specified sound format is not supported.");
            }
        }
        #endregion
	}
	
	public enum SoundType{
		ButtonClick,
		WaterSplash,
		OnOff,
		SwooshSound,
		HitSound,
		NotificationSound,
		FoodEaten,
		ArrowHit,
		BowSound,
		DarkSound,
		SlashSound,
		FootStep,
		Jump,
		TransactionSound,
        FoodEat,
	    Fireplace,
	    HorseRun,
        HumanRun,
        HitGround,
        Dodge,
        LongSwoosh,
        GlassBreak,
	    GlassBreakInverted,
        HumanSleep,
	    MaxSounds
    }
}
