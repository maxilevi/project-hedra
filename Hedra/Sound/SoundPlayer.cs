/*
 * Author: Zaphyk
 * Date: 02/03/2016
 * Time: 01:19 a.m.
 *
 */

using System;
using System.Reflection;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.Sound;
using OpenToolkit.Mathematics;
using OpenToolkit.OpenAL;

namespace Hedra.Sound
{
    /// <summary>
    /// Use for playing sound effects.
    /// </summary>
    public static class SoundPlayer
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
            Provider.Setup();
        }

        public static void Update(Vector3 Position)
        {
            Provider.Update(Position);
        }

        public static void PlaySound(SoundType Sound, Vector3 Location, bool Looping = false, float Pitch = 1, float Gain = 1)
        {
            PlaySound(Sound.ToString(), Location, Looping, Pitch, Gain);
        }
        
        public static void PlaySound(string SoundName, Vector3 Location, bool Looping = false, float Pitch = 1, float Gain = 1)
        {
            Provider.PlaySound(SoundName, Location, Looping, Pitch, Gain);
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
            Provider.PlaySoundWhile(Sound.ToString(), Lambda, Pitch, Gain);
        }

        public static SoundBuffer GetBuffer(SoundType Type)
        {
            return Provider.GetBuffer(Type.ToString());
        }

        public static SoundItem GetAvailableSource()
        {
            return Provider.GetAvailableSource();
        }

        public static BufferFormat GetSoundFormat(int Channels, int Bits)
        {
            return Provider.GetSoundFormat(Channels, Bits);
        }

        public static void LoadSound(SoundType Name, params string[] Names)
        {
            Provider.LoadSound(Name.ToString(), Names);
        }
        
        public static void LoadSound(string Name, params string[] Names)
        {
            Provider.LoadSound(Name, Names);
        }

        public static void MarkAsReady()
        {
            Provider.MarkAsReady();
        }
    }
    
    public enum SoundType
    {
        None,
        ButtonClick,
        WaterSplash,
        ButtonHover,
        SwooshSound,
        HitSound,
        NotificationSound,
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
        HumanRunWood,
        HitGround,
        Dodge,
        LongSwoosh,
        GlassBreak,
        GlassBreakInverted,
        HumanSleep,
        TalkSound,
        GroundQuake,
        BeetleSpitSound,
        GorillaGrowl,
        PreparingAttack,
        River,
        BoatMove,
        Underwater,
        Sheep,
        Goat,
        Cow,
        Door,
        ItemEquip,
        BearTrap,
        HealSound,
        TeleportSound,
        MaxSounds
    }
}
