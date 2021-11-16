using System;
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