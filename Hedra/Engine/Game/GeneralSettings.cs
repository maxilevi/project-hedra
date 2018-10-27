﻿/*
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
		public const float Lod1DistanceSquared = 288 * 288;
		public const float Lod2DistanceSquared = 512 * 512;
		public const float Lod3DistanceSquared = 1024 * 1024;
		public const float LodElementsDistanceSquared = 288 * 288;
		public const float MaxLodDitherDistance = 256;
		public const float MinLodDitherDistance = 228;
		public const int MaxWeights = 3;
	}
}
