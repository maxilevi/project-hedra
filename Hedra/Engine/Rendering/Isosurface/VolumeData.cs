using System.Collections.Generic;
using Hedra.Engine.Generation;

namespace Hedra.Engine.Rendering.Isosurface
{
    public class VolumeData
    {
        private readonly Block[][][] _samples;
        public VolumeData(Block[][][] Samples)
        {
            _samples = Samples;
        }

        public sbyte this[Vector3i p]
        {
            get
            {
                return (sbyte) (_samples[p.X][p.Y][p.Z].Density > 0 ? 1 : -1);       
            }
        }
    }
}