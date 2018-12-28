namespace Hedra.Engine.QuestSystem
{
    public interface IQuest<out T>
    {
        T Instance { get; }
    }
}