using Hedra.Engine.Player;

namespace Hedra.Components
{
    public class TravellingMerchantComponent : ScriptedTradeComponent
    {
        public TravellingMerchantComponent(Humanoid Parent) : base(Parent)
        {
        }

        protected override string BuildInventoryFunctionName => "build_travelling_merchant_inventory";
    }
}