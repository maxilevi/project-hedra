/*
 * Author: Zaphyk
 * Date: 30/01/2016
 * Time: 02:43 p.m.
 *
 */

using System;
using System.Collections.Generic;
using System.Threading;
using Hedra.Game;

namespace Hedra.Core
{
    /// <summary>
    ///     Time manager.
    /// </summary>
    public static class Time
    {
        private static readonly Dictionary<int, TimeProvider> Providers = new Dictionary<int, TimeProvider>();
        private static readonly object Lock = new object();
        public static float TimeScale { get; private set; } = 1;
        public static bool Paused => TimeScale <= 0.005f;
        private static TimeProvider Current
        {
            get
            {
                lock (Lock)
                {
                    return Providers[Thread.CurrentThread.ManagedThreadId];
                }
            }
        }

        public static int Framerate => Current.Framerate;
        public static float Frametime => Current.Frametime;
        public static float DeltaTime => Current.DeltaTime;
        public static float IndependentDeltaTime => Current.IndependentDeltaTime;
        public static float AccumulatedFrameTime => Current.AccumulatedFrameTime;
        public static float IndependentAccumulatedFrameTime => Current.IndependentAccumulatedFrameTime;
        public static float LastFrameUpdate => Current.LastFrameUpdate;

        public static void IncrementFrame(double Time)
        {
            IncrementFrame((float)Time);
        }

        public static void IncrementFrame(float Time)
        {
            Current.IncrementFrame(Time);
        }

        public static void Set(double Time, bool UpdateCounter = true)
        {
            Set((float)Time, UpdateCounter);
        }

        public static void Set(float Time, bool UpdateCounter)
        {
            Current.Set(Time, UpdateCounter);
        }

        public static void RegisterThread()
        {
            lock (Lock)
            {
                if (!Providers.ContainsKey(Thread.CurrentThread.ManagedThreadId))
                    Providers.Add(Thread.CurrentThread.ManagedThreadId, new TimeProvider());
            }
        }

        public static TimeProvider GetProvider(int Id)
        {
            lock (Lock)
            {
                return Providers[Id];
            }
        }

        public class TimeProvider
        {
            public int Framerate { get; private set; }
            public float Frametime { get; private set; }
            public float DeltaTime { get; private set; }
            public float IndependentDeltaTime { get; private set; }
            public float AccumulatedFrameTime { get; private set; }
            public float IndependentAccumulatedFrameTime { get; private set; }
            public float LastFrameUpdate { get; private set; }

            public void Set(float Time, bool UpdateCounter)
            {
                TimeScale = GameSettings.Paused ? 0 : 1;
                IndependentDeltaTime = Time;
                DeltaTime = IndependentDeltaTime * TimeScale;
                if (Math.Abs(LastFrameUpdate - Environment.TickCount) > 1000 && UpdateCounter)
                {
                    Framerate = (int)(1.0 / Time);
                    Frametime = Time;
                    LastFrameUpdate = Environment.TickCount;
                }
            }

            public void IncrementFrame(float Time)
            {
                AccumulatedFrameTime += Time * TimeScale;
                IndependentAccumulatedFrameTime += Time;
            }
        }
    }
}