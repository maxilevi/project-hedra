using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;

namespace Hedra.ItemHandlers
{
    public abstract class ItemHandler
    {
        public abstract bool Consume(IPlayer Owner, Item Item);
    }
}