using System;
using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;

namespace Hedra.Components
{
    public abstract class SingularComponent<T, U> : Component<U>
        where U : IEntity
        where T : Component<U>
    {
        protected SingularComponent(U Entity) : base(Entity)
        {
            AssertSingularity();
        }

        private void AssertSingularity()
        {
            if(Parent.SearchComponent<T>() != null)
                throw new ArgumentException($"An {typeof(U).Name} cannot have more than 1 {typeof(T).Name}.");
        }
    }
}