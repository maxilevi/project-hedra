namespace Hedra.Engine.Generation.ChunkSystem.Builders
{
    public class StructuresBuilder : AbstractBuilder
    {
        public StructuresBuilder(SharedWorkerPool Pool) : base(Pool)
        {
        }

        protected override void Work(Chunk Object)
        {
            Object.GenerateStructures();
        }

        protected override int SleepTime => 5;
        
        protected override QueueType Type => QueueType.Structures;
    }
}