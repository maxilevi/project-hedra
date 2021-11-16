/*
 * Author: Zaphyk
 * Date: 18/02/2016
 * Time: 05:11 p.m.
 *
 */

namespace Hedra.Engine.Generation.ChunkSystem.Builders
{
    public class MeshBuilder : AbstractBuilder
    {
        public MeshBuilder(SharedWorkerPool Pool) : base(Pool)
        {
        }

        protected override QueueType Type => QueueType.Meshing;

        protected override void Work(Chunk Object)
        {
            Object.BuildMesh();
        }
    }
}