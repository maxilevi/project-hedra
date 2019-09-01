namespace Hedra.Engine.Player
{
    public interface IVehicle
    {
        void Update();

        void Enable();

        void Disable();

        bool CanEnable { get; }
        
        bool Enabled { get; }

        void Dispose();
    }
}