using System;
using Hedra.Sound;
using System.Numerics;
using Silk.NET.OpenAL;

namespace Hedra.Engine.Sound
{
    public interface ISoundProvider
    {
        float Volume { get; set; }
        Vector3 ListenerPosition { get; }
        void Setup();
        void Update(Vector3 Position);
        void PlaySound(string Sound, Vector3 Location, bool Looping = false, float Pitch = 1, float Gain = 1);
        void PlaySoundWhile(string Sound, Func<bool> Lambda, Func<float> Pitch, Func<float> Gain);
        SoundBuffer GetBuffer(string Type);
        SoundItem GetAvailableSource();
        BufferFormat GetSoundFormat(int Channels, int Bits);
        void LoadSound(string Name, params string[] Names);
        void MarkAsReady();
    }
}