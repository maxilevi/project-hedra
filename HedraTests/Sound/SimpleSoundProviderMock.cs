using System;
using Hedra.Engine.Sound;
using OpenTK;
using OpenTK.Audio.OpenAL;

namespace HedraTests.Sound
{
    public class SimpleSoundProviderMock : ISoundProvider
    {
        public float Volume { get; set; }
        public Vector3 ListenerPosition { get; private set; }
        
        public void Load()
        {
        }

        public void Update(Vector3 Position)
        {
            ListenerPosition = Position;
        }

        public void PlaySound(SoundType Sound, Vector3 Location, bool Looping = false, float Pitch = 1, float Gain = 1)
        {
        }

        public void PlayUISound(SoundType Sound, float Pitch = 1, float Gain = 1)
        {
        }

        public void PlaySoundWithVariation(SoundType Sound, Vector3 Location, float BasePitch = 1, float BaseGain = 1)
        {
        }

        public void PlaySoundWhile(SoundType Sound, Func<bool> Lambda, float Pitch = 1, float Gain = 1)
        {
        }

        public SoundBuffer GetBuffer(SoundType Type)
        {
            return null;
        }

        public SoundItem GetAvailableSource()
        {
            return null;
        }

        public short[] LoadOgg(string File, out int Channels, out int Bits, out int Rate, out int Count)
        {
            Channels = 0;
            Bits = 0;
            Rate = 0;
            Count = 0;
            return new short[0];
        }

        public short[] LoadOgg(string File, out int Channels, out int Bits, out int Rate, out int BytesPerSecond, int Offset,
            int Length)
        {
            Channels = 0;
            Bits = 0;
            Rate = 0;
            BytesPerSecond = 0;
            return new short[0];
        }

        public byte[] LoadWave(string File, out int Channels, out int Bits, out int Rate)
        {
            Channels = 0;
            Bits = 0;
            Rate = 0;
            return new byte[0];
        }

        public ALFormat GetSoundFormat(int Channels, int Bits)
        {
            return default(ALFormat);
        }
    }
}