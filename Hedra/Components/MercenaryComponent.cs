using System;
using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;

namespace Hedra.Components
{
    public class MercenaryComponent : Component<IEntity>
    {
        public MercenaryComponent(IEntity Entity) : base(Entity)
        {
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }
    }
}