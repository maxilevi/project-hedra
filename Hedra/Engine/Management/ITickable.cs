namespace Hedra.Engine.Management
{
    public interface ITickable
    {
        int UpdatesPerSecond { get; }
        void Update(float DeltaTime);
    }
}