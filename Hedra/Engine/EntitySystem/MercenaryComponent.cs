using Hedra.Engine.Player;

namespace Hedra.Engine.EntitySystem
{
    public class MercenaryComponent : Component<IEntity>
    {
        public MercenaryComponent(IEntity Entity) : base(Entity)
        {
        }

        public override void Update()
        {
            throw new System.NotImplementedException();
        }
    }
}