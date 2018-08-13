/*
 * Author: Zaphyk
 * Date: 30/01/2016
 * Time: 02:43 p.m.
 *
 */
using System;

namespace Hedra.Engine
{
	/// <summary>
	/// Time manager.
	/// </summary>
	public static class Time
	{
		public static float DeltaTime { get; private set; }
		public static float IndependantDeltaTime { get; private set; }
		public static float TimeScale { get; private set; } = 1;
	    public static float AccumulatedFrameTime { get; private set; }
	    public static float IndependentAccumulatedFrameTime { get; private set; }

        public static void IncrementFrame(double Time)
	    {
	        IncrementFrame((float)Time);
	    }

        public static void IncrementFrame(float Time)
	    {
	        AccumulatedFrameTime += Time * TimeScale;
	        IndependentAccumulatedFrameTime += Time;

	    }

	    public static void Set(double Time)
	    {
	        Set((float) Time);
	    }

	    public static void Set(float Time)
	    {
	        TimeScale = GameSettings.Paused && GameManager.InStartMenu ? 0 : 1;
	        IndependantDeltaTime = Time;
	        DeltaTime = IndependantDeltaTime * TimeScale;
        }
	}
}
