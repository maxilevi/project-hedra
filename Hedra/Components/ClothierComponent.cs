using Hedra.EntitySystem;

namespace Hedra.Components
{
    public class ClothierComponent : ScriptedTradeComponent
    {
        public ClothierComponent(IHumanoid Parent) : base(Parent)
        {
        }

        protected override string BuildInventoryFunctionName => "build_clothier_inventory";
    }
}