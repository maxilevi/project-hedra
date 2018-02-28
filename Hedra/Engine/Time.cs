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
		public static double deltaTime;
		public static float unScaledDeltaTime;
		public static double timeScale = 1;
		public static float FrameTimeSeconds = 0;
		public static float ScaledFrameTimeSeconds = 0;
		public static float CurrentFrame = 0, UnPausedCurrentFrame;
	}
}
