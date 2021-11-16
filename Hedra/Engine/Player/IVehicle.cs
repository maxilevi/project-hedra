namespace Hedra.Engine.Player
{
    public interface IVehicle
    {
        bool CanEnable { get; }

        bool Enabled { get; }
        void Update();

        void Enable();

        void Disable();

        void Dispose();
    }
}