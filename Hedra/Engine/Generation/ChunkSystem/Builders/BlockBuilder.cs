namespace Hedra.Engine.Generation.ChunkSystem.Builders
{
    public class BlockBuilder : AbstractBuilder
    {
        public BlockBuilder(SharedWorkerPool Pool) : base(Pool)
        {
        }

        protected override void Work(Chunk Object)
        {
            Object.GenerateBlocks();
        }

        protected override int SleepTime => 5;
    }
}