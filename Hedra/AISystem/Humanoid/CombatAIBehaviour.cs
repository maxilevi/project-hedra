using System.Numerics;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Humanoid
{
    public abstract class CombatAIBehaviour
    {
        protected CombatAIBehaviour(IHumanoid Parent)
        {
            this.Parent = Parent;
        }

        protected IHumanoid Parent { get; }

        public abstract float WaitTime { get; }

        public abstract Vector3 FindPoint();

        public abstract IEntity FindPlayerTarget(float SearchRadius);

        public abstract IEntity FindMobTarget(float SearchRadius);

        public virtual void OnStare(IEntity Entity)
        {
        }

        public virtual void OnStuck()
        {
        }

        public virtual void SetTarget(IEntity Entity)
        {
        }
    }
}