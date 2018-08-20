using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.PhysicsSystem
{
    public interface IPhysicsThreadManager
    {
        event OnCommandProcessedEventHandler OnCommandProcessedEvent;
        event OnBatchProcessedEventHandler OnBatchProcessedEvent;
        int Count { get; }
        void Load();
        void AddCommand(Entity Member);
        void AddCommand(MoveCommand Member);
        void Update();
    }
}