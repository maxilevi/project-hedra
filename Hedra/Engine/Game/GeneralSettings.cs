/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 25/03/2017
 * Time: 08:52 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace Hedra.Engine.Game
{
	/// <summary>
	/// Description of GeneralSettings.
	/// </summary>
	public static class GeneralSettings
	{
		public static bool CollectPerformanceMetrics = false;
		public static int MaxWeights = 3;
	    public static int MaxJoints = 50;

		static GeneralSettings()
		{
			#if DEBUG
			CollectPerformanceMetrics = true;
			#endif
		}
	}
}
