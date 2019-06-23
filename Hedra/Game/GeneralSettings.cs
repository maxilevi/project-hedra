/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 25/03/2017
 * Time: 08:52 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using OpenTK;

namespace Hedra.Game
{
    /// <summary>
    /// Description of GeneralSettings.
    /// </summary>
    public static class GeneralSettings
    {
        public static readonly Vector3 SpawnPoint = new Vector3(5000, 0, 5000);
        public const float DrawDistanceSquared = 512 * 512;
        public const float UpdateDistanceSquared = 420 * 420;
        public const float Lod1DistanceSquared = 288 * 288;
        public const float Lod2DistanceSquared = 512 * 512;
        public const float Lod3DistanceSquared = 1024 * 1024;
        public const float MaxLodDitherDistance = 256;
        public const float MinLodDitherDistance = 200;
        public const int MaxWeights = 3;
        public const int MaxLoadingRadius = 32;
        public const int MinLoadingRadius = 8;
        public const int MaxChunks = MaxLoadingRadius * MaxLoadingRadius;
    }
}
