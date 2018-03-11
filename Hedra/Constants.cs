/*
 * Author: Zaphyk
 * Date: 05/02/2016
 * Time: 06:33 p.m.
 *
 */
using System;
using OpenTK;

namespace Hedra
{
	/// <summary>
	/// Description of Constants.
	/// </summary>
	[Obsolete]
	public static class Constants
	{
		public static int VIEW_DISTANCE = 1024;
		public static int UPDATE_DISTANCE = 256;
		public static int WIDTH = 1024, DEVICE_WIDTH;
		public static int HEIGHT = 578, DEVICE_HEIGHT;
		public static float SCREEN_RATIO = (float) ( (float) WIDTH / (float) HEIGHT);
		public static float DEFAULT_SCREEN_RATIO = 1.33f;
		public static bool DEBUG = true;
		public static bool LINES = false;
		public static bool LOCK_FRUSTUM = false;
		public static bool PAUSED = false;
		public static bool CHARACTER_CHOOSED = false;
		public static bool CLAMP_CAMERA = true;
		public static bool REDIRECT_NET = false;
		public static bool REDIRECT_NEW_RUN = false;
		public static bool HIDE_CHUNKS = false;
		public static bool HIDE_ENTITIES = false;
		public static bool NO_CULL = false;
		public static bool COLLIDES;
	}
}
