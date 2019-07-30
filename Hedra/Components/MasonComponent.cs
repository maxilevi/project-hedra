using Hedra.Engine.Player;
using Hedra.EntitySystem;

namespace Hedra.Components
{
    public class MasonComponent : ScriptedTradeComponent
    {
        public MasonComponent(IHumanoid Parent) : base(Parent)
        {
        }

        protected override string BuildInventoryFunctionName => "build_mason_inventory";
    }
}