namespace Hedra.Engine.Generation.ChunkSystem
{
    public class ChunkBuilder : AbstractBuilder
    {
        public ChunkBuilder(SharedWorkerPool Pool) : base(Pool)
        {
        }

        protected override void Work(Chunk Object)
        {
            Object?.Generate();
        }

        protected override int SleepTime => 15;
    }
}
