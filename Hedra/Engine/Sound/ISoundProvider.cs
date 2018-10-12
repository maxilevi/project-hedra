using System;
using OpenTK;
using OpenTK.Audio.OpenAL;

namespace Hedra.Engine.Sound
{
    public interface ISoundProvider
    {
        float Volume { get; set; }
        Vector3 ListenerPosition { get; }
        void Load();
        void Update(Vector3 Position);
        void PlaySound(SoundType Sound, Vector3 Location, bool Looping = false, float Pitch = 1, float Gain = 1);
        void PlaySoundWhile(SoundType Sound, Func<bool> Lambda, Func<float> Pitch, Func<float> Gain);
        SoundBuffer GetBuffer(SoundType Type);
        SoundItem GetAvailableSource();
        short[] LoadOgg(string File, out int Channels, out int Bits, out int Rate, out int Count);
        short[] LoadOgg(string File, out int Channels, out int Bits, out int Rate, out int BytesPerSecond, int Offset, int Length);
        byte[] LoadWave(string File, out int Channels, out int Bits, out int Rate);
        ALFormat GetSoundFormat(int Channels, int Bits);
    }
}