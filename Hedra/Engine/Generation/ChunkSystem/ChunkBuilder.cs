namespace Hedra.Engine.Generation.ChunkSystem
{
    internal class ChunkBuilder : AbstractBuilder
    {
        public ChunkBuilder(SharedWorkerPool Pool) : base(Pool)
        {
        }

        protected override void Work(Chunk Object)
        {
            Log.RunWithType(LogType.WorldBuilding, delegate
            {
                if ((!Object?.Mesh?.IsGenerated ?? false) || (Object?.Landscape?.BlocksSetted ?? false))
                {
                    Object?.Generate();
                }
            });
        }
    }
}
