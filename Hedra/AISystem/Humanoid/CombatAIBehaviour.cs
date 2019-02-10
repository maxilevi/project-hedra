using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.AISystem.Humanoid
{
    public abstract class CombatAIBehaviour
    {
        protected IHumanoid Parent { get; }

        protected CombatAIBehaviour(IHumanoid Parent)
        {
            this.Parent = Parent;
        }

        public abstract float WaitTime { get; }
        
        public abstract Vector3 FindPoint();
        
        public abstract IEntity FindPlayerTarget(float SearchRadius);

        public abstract IEntity FindMobTarget(float SearchRadius);

        public virtual void OnStare(IEntity Entity)
        {
            
        }
    }
}