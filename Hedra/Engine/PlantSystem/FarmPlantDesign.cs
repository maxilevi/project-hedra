using System;
using Hedra.BiomeSystem;
using Hedra.Rendering;
using System.Numerics;

namespace Hedra.Engine.PlantSystem
{
    public abstract class FarmPlantDesign : PlantDesign
    {
        public override Matrix4x4 TransMatrix(Vector3 Position, Random Rng)
        {
            throw new NotImplementedException();
        }
    }
}