using System;
using System.Numerics;
using Silk.NET.OpenAL;

namespace Hedra.Engine.Sound
{
    public class DummySoundProvider : ISoundProvider
    {
        public float Volume { get; set; }
        public Vector3 ListenerPosition { get; private set; }
        
        public void Setup()
        {
        }

        public void Update(Vector3 Position)
        {
            ListenerPosition = Position;
        }

        public void PlaySound(string Sound, Vector3 Location, bool Looping = false, float Pitch = 1, float Gain = 1)
        {
        }

        public void PlaySoundWhile(string Sound, Func<bool> Lambda, Func<float> Pitch, Func<float> Gain)
        {
        }

        public SoundBuffer GetBuffer(string Type)
        {
            return null;
        }

        public SoundItem GetAvailableSource()
        {
            return null;
        }

        public BufferFormat GetSoundFormat(int Channels, int Bits)
        {
            return default(BufferFormat);
        }

        public void LoadSound(string Name, params string[] Names)
        {
        }

        public void MarkAsReady()
        {
        }
    }
}