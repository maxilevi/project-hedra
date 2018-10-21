/*
 * Author: Zaphyk
 * Date: 30/01/2016
 * Time: 02:43 p.m.
 *
 */
using System;
using Hedra.Engine.Game;

namespace Hedra.Engine
{
	/// <summary>
	/// Time manager.
	/// </summary>
	public static class Time
	{
		public static int Framerate { get; private set; }
		public static float Frametime { get; private set; } 
	    public static bool Paused => TimeScale <= 0.005f; 
		public static float DeltaTime { get; private set; }
		public static float IndependantDeltaTime { get; private set; }
		public static float TimeScale { get; private set; } = 1;
	    public static float AccumulatedFrameTime { get; private set; }
	    public static float IndependentAccumulatedFrameTime { get; private set; }
		public static float LastFrameUpdate { get; set; }

        public static void IncrementFrame(double Time)
	    {
	        IncrementFrame((float)Time);
	    }

        public static void IncrementFrame(float Time)
	    {
	        AccumulatedFrameTime += Time * TimeScale;
	        IndependentAccumulatedFrameTime += Time;
	    }

	    public static void Set(double Time, bool UpdateCounter = true)
	    {
	        Set((float) Time, UpdateCounter);
	    }

	    public static void Set(float Time, bool UpdateCounter)
	    {
	        TimeScale = GameSettings.Paused ? 0 : 1;
	        IndependantDeltaTime = Time;
	        DeltaTime = IndependantDeltaTime * TimeScale;
		    if (Math.Abs(LastFrameUpdate - Environment.TickCount) > 1000 && UpdateCounter)
		    {
			    Framerate = (int) (1.0 / Time);
			    Frametime = (float) Time;
			    LastFrameUpdate = Environment.TickCount;
		    }
	    }
	}
}
