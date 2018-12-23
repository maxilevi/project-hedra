using System;
using Hedra.BiomeSystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    public abstract class FarmPlantDesign : PlantDesign
    {
        public override Matrix4 TransMatrix(Vector3 Position, Random Rng)
        {
            throw new NotImplementedException();
        }
    }
}