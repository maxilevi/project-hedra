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
	            SoundBuffers[(int) SoundType.FoodEaten] = SoundBuffers[(int) SoundType.NotificationSound];
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
	            LoadSound(SoundType.PreparingAttack, "Sounds/Bow.ogg");
                _loaded = true;
                Log.WriteLine("Finished loading sounds.");
            });
        }

	    private static void LoadSound(SoundType Type, string Name)
	    {
	        SoundBuffers[(int)Type] = 
		        new SoundBuffer(
			        LoadOgg(Name, out var channels, out var bits, out var rate),
			        GetSoundFormat(channels, bits),
			        rate
			        );
        }

        public static void Update(Vector3 Position)
        {
			if(!_loaded) return;
            ListenerPosition = Position;
        }

        public static void PlaySound(SoundType Sound, Vector3 Location, bool Looping = false, float Pitch = 1, float Gain = 1)
        {

            if(!_loaded) return;
            ListenerPosition = LocalPlayer.Instance.Position;

		    Gain = Math.Max(Gain * (1-(ListenerPosition - Location).LengthFast / 256f) * Volume, 0);
            if(Gain <= 0 ) return;

	        var source = GrabSource();
			if(source == null)
			{
				Log.WriteLine("Could not play sound "+ Sound);
				return;
			}
			source.Play(SoundBuffers[ (int) Sound], Location, Pitch, Gain, Looping);			
		}
		
		public static void PlayUISound(SoundType Sound, float Pitch = 1, float Gain = 1)
		{
            if (!_loaded) return;
            PlaySound(Sound, ListenerPosition, false, Pitch, Gain);
		}
		
		public static void PlaySoundWithVariation(SoundType Sound, Vector3 Location, float BasePitch = 1f, float BaseGain = 1f)
		{
            if (!_loaded) return;
            PlaySound(Sound, Location, false, BasePitch + Utils.Rng.NextFloat() * .2f - .1f, BaseGain + Utils.Rng.NextFloat() * .2f - .1f);
		}

		public static void PlaySoundWhile(SoundType Sound, Func<bool> Lambda, float Pitch = 1, float Gain = 1)
		{
			var source = GrabSource();
			if(source == null)
			{
				Log.WriteLine($"Could not play sound {Sound}");
				return;
			}
			TaskManager.While(Lambda, delegate
			{
				if (source.IsPlaying) return;
				source.Play(SoundBuffers[(int) Sound], ListenerPosition, Pitch, Gain, false);
			});
		}

		private static SoundSource GrabSource()
		{
			SoundSource source = null;
			for (var i = 0; i < SoundSources.Length; i++)
			{
				if (SoundSources[i].IsPlaying) continue;
				source = SoundSources[i];
				break;
			}
			return source;
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
        TalkSound,
	    GroundQuake,
        SpitSound,
	    GorillaGrowl,
        PreparingAttack,
        MaxSounds
    }
}
