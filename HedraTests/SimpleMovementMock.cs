using Hedra.Engine.Player;
using Hedra.EntitySystem;

namespace HedraTests
{
    public class SimpleMovementMock : MovementManager
    {
        public SimpleMovementMock(IHumanoid Human) : base(Human)
        {
        }
    }
}