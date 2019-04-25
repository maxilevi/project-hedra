namespace Hedra.Engine.Player
{
    public interface IObjectWithLifeCycle
    {
        bool IsDead { get; set; }
        bool IsKnocked { get; }
    }
}