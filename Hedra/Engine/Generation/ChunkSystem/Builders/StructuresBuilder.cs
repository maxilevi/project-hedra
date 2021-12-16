namespace Hedra.Engine.Generation.ChunkSystem.Builders
{
    public class StructuresBuilder : AbstractBuilder
    {
        public StructuresBuilder(SharedWorkerPool Pool) : base(Pool)
        {
        }

        protected override QueueType Type => QueueType.Structures;

        protected override void Work(Chunk Object)
        {
            if(Object.IsGenerated) return;
            Object.GenerateStructures();
        }
    }
}