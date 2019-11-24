using Hedra.AISystem.Behaviours;

namespace Hedra.AISystem
{
    public interface IBehaviouralAI
    {
        void AlterBehaviour<T>(T NewBehaviour) where T : Behaviour;
        T SearchBehaviour<T>() where T : Behaviour;
    }
}