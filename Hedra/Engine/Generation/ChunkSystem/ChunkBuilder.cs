namespace Hedra.Engine.Generation.ChunkSystem
{
    internal class ChunkBuilder : AbstractBuilder
    {
        public ChunkBuilder(SharedWorkerPool Pool) : base(Pool)
        {
        }

        protected override void Work(Chunk Object)
        {
            Object?.Generate();
        }
    }
}
