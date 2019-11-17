using Hedra.AISystem.Behaviours;

namespace Hedra.AISystem
{
    public interface IAlterableAI
    {
        void AlterBehaviour<T>(T NewBehaviour) where T : Behaviour;
    }
}