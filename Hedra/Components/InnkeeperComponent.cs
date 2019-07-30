using Hedra.Engine.Player;
using Hedra.EntitySystem;

namespace Hedra.Components
{
    public class InnkeeperComponent : ScriptedTradeComponent
    {
        public InnkeeperComponent(IHumanoid Parent) : base(Parent)
        {
        }

        protected override string BuildInventoryFunctionName => "build_innkeeper_inventory";
    }
}