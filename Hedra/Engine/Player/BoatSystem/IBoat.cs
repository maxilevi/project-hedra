namespace Hedra.Engine.Player.BoatSystem
{
    public interface IBoat
    {
        void Update();
        bool Enabled { get; }
    }
}