using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player.Inventory;

namespace HedraTests.ItemSystem
{
    public class TradeManagerMock : TradeManager
    {
        public TradeManagerMock() : base(null, null)
        {
        }

        protected override float GetPriceMultiplier(Item Item)
        {
            return 1;
        }
    }
}