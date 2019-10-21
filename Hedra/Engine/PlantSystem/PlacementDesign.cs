using System;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using System.Numerics;

namespace Hedra.Engine.PlantSystem
{
    /// <summary>
    /// A design for a plant placement.
    /// </summary>
    public abstract class PlacementDesign
    {
        /// <summary>
        /// If the placement can be hidden.
        /// </summary>
        public virtual bool CanBeHidden => false;

        /// <summary>
        /// Plant design to use for this placement design.
        /// </summary>
        public abstract PlantDesign GetDesign(Vector3 Position, Chunk UnderChunk, Random Rng);

        /// <summary>
        /// Function that deterministically says if a placement should be done.
        /// </summary>
        /// <param name="Position">Vector to be used for the seed.</param>
        /// <returns></returns>
        public abstract bool ShouldPlace(Vector3 Position, Chunk UnderChunk);
    }
}
