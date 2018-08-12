using Hedra.Engine.Player;

namespace HedraTests
{
    public class SimpleMovementMock : MovementManager
    {
        public SimpleMovementMock(IHumanoid Human) : base(Human)
        {
        }
    }
}