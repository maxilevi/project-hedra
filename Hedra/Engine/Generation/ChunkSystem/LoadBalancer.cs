namespace Hedra.Engine.Generation.ChunkSystem
{
    public class LoadBalancer
    {
        public LoadBalancer(int Capacity)
        {
            this.Capacity = Capacity;
        }

        public int Capacity { get; }
    }
}