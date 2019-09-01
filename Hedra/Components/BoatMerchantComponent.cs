using Hedra.EntitySystem;

namespace Hedra.Components
{
    public class BoatMerchantComponent : ScriptedTradeComponent
    {
        public BoatMerchantComponent(IHumanoid Parent) : base(Parent)
        {
        }

        protected override string BuildInventoryFunctionName => "build_boat_merchant_inventory";
    }
}