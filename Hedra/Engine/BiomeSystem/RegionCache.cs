using Hedra.Engine.Generation;
using OpenTK;

namespace Hedra.Engine.BiomeSystem
{
    public class RegionCache
    {
        private readonly Region _predominantRegion;
        private readonly RegionColor _predominantInterpolatedRegionColor;
        private readonly Vector3 _min;
        private readonly Vector3 _max;

        public RegionCache(Vector3 Min, Vector3 Max)
        {
            this._min = Min;
            this._max = Max;
            var middle = (Min + Max) * .5f;
            var middleRegion = World.BiomePool.GetRegion(middle);
            if (middleRegion != World.BiomePool.GetRegion(_min)) return;
            if (middleRegion != World.BiomePool.GetRegion(new Vector3(_max.X, 0, _min.Z))) return;
            if (middleRegion != World.BiomePool.GetRegion(new Vector3(_min.X, 0, _max.Z))) return;
            if (middleRegion != World.BiomePool.GetRegion(_max)) return;
            _predominantRegion = middleRegion;
            _predominantInterpolatedRegionColor = World.BiomePool.GetAverageRegionColor(middle);
        }

        public Region GetRegion(Vector3 Position)
        {
            if (_predominantRegion == null || !this.InsideSamplers(Position))
                return World.BiomePool.GetRegion(Position);
            return _predominantRegion;
        }

        public RegionColor GetAverageRegionColor(Vector3 Position)
        {
            if (_predominantInterpolatedRegionColor == null || !this.InsideSamplers(Position))
                return World.BiomePool.GetAverageRegionColor(Position);
            return _predominantInterpolatedRegionColor;
        }

        private bool InsideSamplers(Vector3 Position)
        {
            return Position.X >= _min.X && Position.X <= _max.X &&
                   Position.Z >= _min.Z && Position.Z <= _max.Z;
        }
    }
}
