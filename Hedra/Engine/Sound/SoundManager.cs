/*
 * Author: Zaphyk
 * Date: 02/03/2016
 * Time: 01:19 a.m.
 *
 */
using System;
using System.IO;
using System.Reflection;
using OpenTK;
using OpenTK.Audio.OpenAL;


namespace Hedra.Engine.Sound
{
    /// <summary>
    /// Description of SoundManager.
    /// </summary>
    [Obfuscation(Exclude = false, Feature = "-rename")]
    public static class SoundManager
    {
        public static ISoundProvider Provider { get; set; } = new SoundProvider();

        public static Vector3 ListenerPosition => Provider.ListenerPosition;

        public static float Volume
        {
            get => Provider.Volume;
            set => Provider.Volume = value;
        }
        
        public static void Load()
        {
            Provider.Load();
        }

        public static void Update(Vector3 Position)
        {
            Provider.Update(Position);
        }

        public static void PlaySound(SoundType Sound, Vector3 Location, bool Looping = false, float Pitch = 1, float Gain = 1)
        {
            Provider.PlaySound(Sound, Location, Looping, Pitch, Gain);
        }
        
        public static void PlayUISound(SoundType Sound, float Pitch = 1, float Gain = 1)
        {
            PlaySound(Sound, ListenerPosition, false, Pitch, Gain);
        }
        
        public static void PlaySoundWithVariation(SoundType Sound, Vector3 Location, float BasePitch = 1f, float BaseGain = 1f)
        {
            PlaySound(Sound, Location, false, BasePitch + Utils.Rng.NextFloat() * .2f - .1f, BaseGain + Utils.Rng.NextFloat() * .2f - .1f);
        }

        public static void PlaySoundWhile(SoundType Sound, Func<bool> Lambda, Func<float> Pitch, Func<float> Gain)
        {
            Provider.PlaySoundWhile(Sound, Lambda, Pitch, Gain);
        }

        public static SoundBuffer GetBuffer(SoundType Type)
        {
            return Provider.GetBuffer(Type);
        }

        public static SoundItem GetAvailableSource()
        {
            return Provider.GetAvailableSource();
        }

        public static ALFormat GetSoundFormat(int Channels, int Bits)
        {
            return Provider.GetSoundFormat(Channels, Bits);
        }
    }
    
    [Obfuscation(Exclude = false, Feature = "-rename")]
    public enum SoundType
    {
        None,
        ButtonClick,
        WaterSplash,
        ButtonHover,
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
        River,
        BoatMove,
        Swimming,
        Underwater,
        Sheep,
        Goat,
        Cow,
        MaxSounds
    }
}
